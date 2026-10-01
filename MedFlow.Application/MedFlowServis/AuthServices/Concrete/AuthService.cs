using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Application.MedFlowServis.OtpServices;
using MedFlow.Application.MedFlowServis.OtpServices.Concrete;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.AuthServices.Concrete;

public class AuthService : StatusGenericHandler, IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IOtpService _otpService;

    public AuthService(
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOtpService otpService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _otpService = otpService;
    }

    public async Task<AuthResultDto?> RegisterPatientAsync(RegisterPatientRequest request, CancellationToken ct = default)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.PhoneNumber);

        if (phoneNumber.Length == 0)
        {
            AddError("Telefon raqamini kiriting.");
            return null;
        }

        var verifiedViaTelegram = await _otpService.IsRecentlyVerifiedAsync(phoneNumber, ct);
        if (!verifiedViaTelegram)
        {
            AddError("Iltimos, avval telefon raqamingizni Telegram-kod orqali tasdiqlang.");
            return null;
        }

        if (await _uow.Users.Query().AnyAsync(u => phoneForms.Contains(u.PhoneNumber), ct))
        {
            AddError("Bu telefon raqami bilan foydalanuvchi allaqachon ro'yxatdan o'tgan.");
            return null;
        }

        if (request.ParentId is { } parentId && await _uow.Patients.GetByIdAsync(parentId, ct) is null)
        {
            AddError($"{nameof(Patient)} (Id={parentId}) topilmadi.");
            return null;
        }

        var patient = new Patient
        {
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            Phone = phoneNumber,
            ParentId = request.ParentId,
            IsChild = request.IsChild,
            Allergies = request.Allergies,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Patient va User bitta tranzaksiyada yaratiladi: oraliqda xatolik bo'lsa
        // "orphan" Patient qolib ketmaydi.
        var user = await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _uow.Patients.AddAsync(patient, ct);
            await _uow.SaveChangesAsync(ct);

            var newUser = new User
            {
                Role = UserRole.Patient,
                FullName = request.FullName,
                PhoneNumber = phoneNumber,
                PasswordHash = _passwordHasher.Hash(request.Password),
                LinkedPatientId = patient.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _uow.Users.AddAsync(newUser, ct);
            await _uow.SaveChangesAsync(ct);

            // Tasdiqlash bir martalik: shu OTP bilan keyin parolni tiklab bo'lmasin.
            await _otpService.ConsumeVerificationAsync(phoneNumber, ct);

            return newUser;
        }, ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResultDto?> RegisterAdminAsync(RegisterAdminRequest request, bool callerIsAdmin, CancellationToken ct = default)
    {
        var anyAdminExists = await _uow.Users.Query().AnyAsync(u => u.Role == UserRole.Admin, ct);
        if (anyAdminExists && !callerIsAdmin)
        {
            AddError("Admin akkount yaratish uchun mavjud Admin sifatida tizimga kirgan bo'lishingiz shart.");
            return null;
        }

        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.PhoneNumber);

        if (phoneNumber.Length == 0)
        {
            AddError("Telefon raqamini kiriting.");
            return null;
        }

        if (await _uow.Users.Query().AnyAsync(u => phoneForms.Contains(u.PhoneNumber), ct))
        {
            AddError("Bu telefon raqami bilan foydalanuvchi allaqachon ro'yxatdan o'tgan.");
            return null;
        }

        var user = new User
        {
            Role = UserRole.Admin,
            FullName = request.FullName,
            PhoneNumber = phoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResultDto?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // Eski ("+"siz) va yangi ("+998...") yozuvlar bilan mos kelishi uchun
        // ikkala ko'rinish bo'yicha qidiramiz.
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.PhoneNumber);
        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => phoneForms.Contains(u.PhoneNumber), ct);
        if (user is null)
        {
            AddError("Login yoki parol xato.");
            return null;
        }

        if (user.LockedUntil is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
        {
            // Soat emas, qolgan daqiqa ko'rsatiladi: LockedUntil UTC'da saqlanadi va
            // foydalanuvchiga mahalliy vaqtdan 5 soat farqli soat chiqib qolardi.
            var minutesLeft = (int)Math.Ceiling((lockedUntil - DateTimeOffset.UtcNow).TotalMinutes);
            AddError($"Hisob vaqtincha bloklangan. {minutesLeft} daqiqadan keyin qayta urinib ko'ring.");
            return null;
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount += 1;
            if (user.FailedLoginCount >= AuthConstants.MaxFailedLoginAttempts)
            {
                user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(AuthConstants.LockoutMinutes);
                user.FailedLoginCount = 0;
            }

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);

            AddError("Login yoki parol xato.");
            return null;
        }

        if (!await IsAccountActiveAsync(user, ct))
        {
            AddError("Hisob faolsizlantirilgan. Administratorga murojaat qiling.");
            return null;
        }

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResultDto?> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);

        var existing = await _uow.RefreshTokens.Query()
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

        if (existing is null || existing.RevokedAt is not null || existing.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            AddError("Refresh token yaroqsiz yoki muddati o'tgan.");
            return null;
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;
        _uow.RefreshTokens.Update(existing);

        var user = await _uow.Users.GetByIdAsync(existing.UserId, ct);
        if (user is null)
        {
            AddError($"{nameof(User)} (Id={existing.UserId}) topilmadi.");
            return null;
        }

        await _uow.SaveChangesAsync(ct);

        if (!await IsAccountActiveAsync(user, ct))
        {
            AddError("Hisob faolsizlantirilgan. Administratorga murojaat qiling.");
            return null;
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<OtpResultDto?> SendOtpAsync(SendOtpRequest request, CancellationToken ct = default)
    {
        var ok = await _otpService.SendOtpAsync(request, ct);
        if (!ok)
        {
            CopyErrorsFrom(_otpService);
            return null;
        }

        return new OtpResultDto { Success = true };
    }

    public async Task<OtpResultDto?> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var ok = await _otpService.VerifyOtpAsync(request, ct);
        if (!ok)
        {
            CopyErrorsFrom(_otpService);
            return null;
        }

        return new OtpResultDto { Success = true };
    }

    public async Task<AuthResultDto?> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.PhoneNumber);

        var verifiedViaTelegram = await _otpService.IsRecentlyVerifiedAsync(phoneNumber, ct);
        if (!verifiedViaTelegram)
        {
            AddError("Iltimos, avval telefon raqamingizni Telegram-kod orqali tasdiqlang.");
            return null;
        }

        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => phoneForms.Contains(u.PhoneNumber), ct);
        if (user is null)
        {
            AddError("Bu telefon raqami bilan foydalanuvchi topilmadi.");
            return null;
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        _uow.Users.Update(user);

        // Parol o'zgargach eski seanslar (refresh tokenlar) yaroqsiz bo'ladi - aks holda
        // parolni bilib olgan odam tiklashdan keyin ham tizimda qolaverardi.
        var activeTokens = await _uow.RefreshTokens.Query()
            .Where(r => r.UserId == user.Id && r.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            _uow.RefreshTokens.Update(token);
        }

        await _uow.SaveChangesAsync(ct);

        // Tasdiqlash bir martalik: shu OTP bilan parolni yana tiklab bo'lmasin.
        await _otpService.ConsumeVerificationAsync(phoneNumber, ct);

        return await IssueTokensAsync(user, ct);
    }

    /// <summary>Ishdan bo'shatilgan (faolsizlantirilgan) hamshira tizimga kira olmaydi.</summary>
    private async Task<bool> IsAccountActiveAsync(User user, CancellationToken ct)
    {
        if (user.LinkedNurseId is not { } nurseId)
        {
            return true;
        }

        var nurse = await _uow.Nurses.GetByIdAsync(nurseId, ct);
        return nurse is null || nurse.IsActive;
    }

    private void CopyErrorsFrom(IStatusGeneric status)
    {
        foreach (var error in status.Errors)
        {
            AddError(error.ErrorResult.ErrorMessage ?? "Xatolik yuz berdi.");
        }
    }

    private async Task<AuthResultDto> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, expiresAt) = _jwtTokenService.CreateAccessToken(user);
        var (rawRefreshToken, refreshTokenHash, refreshExpiresAt) = _jwtTokenService.CreateRefreshToken();

        await _uow.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAt = expiresAt,
            Role = user.Role,
            Panel = ToPanel(user.Role),
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            LinkedPatientId = user.LinkedPatientId,
            LinkedNurseId = user.LinkedNurseId
        };
    }

    private static string ToPanel(UserRole role) => role switch
    {
        UserRole.Patient => "Patient",
        UserRole.PediatricNurse or UserRole.AdultNurse => "Nurse",
        UserRole.Admin => "Admin",
        _ => "Unknown"
    };
}
