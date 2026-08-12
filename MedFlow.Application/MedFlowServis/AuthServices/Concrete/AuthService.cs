using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
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

    public AuthService(IUnitOfWork uow, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResultDto?> RegisterPatientAsync(RegisterPatientRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.Query().AnyAsync(u => u.PhoneNumber == request.PhoneNumber, ct))
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
            Phone = request.PhoneNumber,
            ParentId = request.ParentId,
            IsChild = request.IsChild,
            Allergies = request.Allergies,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _uow.Patients.AddAsync(patient, ct);
        await _uow.SaveChangesAsync(ct);

        var user = new User
        {
            Role = UserRole.Patient,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            LinkedPatientId = patient.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

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

        if (await _uow.Users.Query().AnyAsync(u => u.PhoneNumber == request.PhoneNumber, ct))
        {
            AddError("Bu telefon raqami bilan foydalanuvchi allaqachon ro'yxatdan o'tgan.");
            return null;
        }

        var user = new User
        {
            Role = UserRole.Admin,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResultDto?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, ct);
        if (user is null)
        {
            AddError("Login yoki parol xato.");
            return null;
        }

        if (user.LockedUntil is { } lockedUntil && lockedUntil > DateTimeOffset.UtcNow)
        {
            AddError($"Hisob vaqtincha bloklangan. {lockedUntil:HH:mm}dan keyin qayta urinib ko'ring.");
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

        return await IssueTokensAsync(user, ct);
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
