using System.Security.Cryptography;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.OtpServices.Concrete;

public class OtpService : StatusGenericHandler, IOtpService
{
    private readonly IUnitOfWork _uow;
    private readonly ITelegramOtpSender _telegramSender;

    public OtpService(IUnitOfWork uow, ITelegramOtpSender telegramSender)
    {
        _uow = uow;
        _telegramSender = telegramSender;
    }

    public async Task<bool> SendOtpAsync(SendOtpRequest request, CancellationToken ct = default)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.PhoneNumber);
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.PhoneNumber);

        if (phoneNumber.Length == 0)
        {
            AddError("Telefon raqamini kiriting.");
            return false;
        }

        // TELEGRAM_LINKS.PHONE_NUMBER bot tomonidan "+998..." ko'rinishida saqlanadi,
        // shuning uchun ham "+"li, ham "+"siz saqlangan yozuvlar bo'yicha izlaymiz.
        var link = await _uow.TelegramLinks.Query().FirstOrDefaultAsync(l => phoneForms.Contains(l.PhoneNumber), ct);
        if (link is null)
        {
            AddError("Bu raqam Telegram botga ulanmagan. Avval botga '/start' bosib, telefon raqamingizni ulashing.");
            return false;
        }

        var lastCode = await _uow.OtpCodes.Query()
            .Where(o => phoneForms.Contains(o.PhoneNumber))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (lastCode is not null &&
            lastCode.CreatedAt.AddSeconds(AuthConstants.OtpResendCooldownSeconds) > DateTimeOffset.UtcNow)
        {
            var wait = (int)(lastCode.CreatedAt.AddSeconds(AuthConstants.OtpResendCooldownSeconds) - DateTimeOffset.UtcNow).TotalSeconds;
            AddError($"Iltimos, {wait} soniyadan keyin qayta urinib ko'ring.");
            return false;
        }

        var code = GenerateCode(AuthConstants.OtpCodeLength);
        var otp = new OtpCode
        {
            PhoneNumber = phoneNumber,
            Code = code,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(AuthConstants.OtpExpiryMinutes),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _uow.OtpCodes.AddAsync(otp, ct);
        await _uow.SaveChangesAsync(ct);

        var sent = await _telegramSender.SendMessageAsync(
            link.ChatId,
            $"🔐 MedFlow tasdiqlash kodi: {code}\n\nKod {AuthConstants.OtpExpiryMinutes} daqiqa amal qiladi. Hech kimga aytmang.",
            ct);

        if (!sent)
        {
            AddError("Telegram orqali xabar yuborib bo'lmadi. Botni bloklamaganingizga ishonch hosil qiling.");
            return false;
        }

        return true;
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.PhoneNumber);
        var code = request.Code.Trim();

        var otp = await _uow.OtpCodes.Query()
            .Where(o => phoneForms.Contains(o.PhoneNumber) && !o.IsUsed && o.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otp is null)
        {
            AddError("Kod topilmadi yoki muddati o'tgan. Yangi kod so'rang.");
            return false;
        }

        if (otp.AttemptCount >= AuthConstants.OtpMaxVerifyAttempts)
        {
            AddError("Urinishlar soni tugadi. Yangi kod so'rang.");
            return false;
        }

        if (otp.Code != code)
        {
            otp.AttemptCount += 1;
            _uow.OtpCodes.Update(otp);
            await _uow.SaveChangesAsync(ct);

            AddError("Kod noto'g'ri.");
            return false;
        }

        otp.IsUsed = true;
        otp.VerifiedAt = DateTimeOffset.UtcNow;
        _uow.OtpCodes.Update(otp);
        await _uow.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> IsRecentlyVerifiedAsync(string phoneNumber, CancellationToken ct = default)
    {
        var phoneForms = PhoneNumberNormalizer.StorageForms(phoneNumber);
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-AuthConstants.OtpExpiryMinutes);

        return await _uow.OtpCodes.Query()
            .AnyAsync(o => phoneForms.Contains(o.PhoneNumber) && o.IsUsed && o.VerifiedAt != null && o.VerifiedAt > cutoff, ct);
    }

    public async Task ConsumeVerificationAsync(string phoneNumber, CancellationToken ct = default)
    {
        var phoneForms = PhoneNumberNormalizer.StorageForms(phoneNumber);

        var verifiedCodes = await _uow.OtpCodes.Query()
            .Where(o => phoneForms.Contains(o.PhoneNumber) && o.IsUsed && o.VerifiedAt != null)
            .ToListAsync(ct);

        // VerifiedAt bo'shatilgach IsRecentlyVerifiedAsync bu kodni qayta "tasdiqlangan" deb hisoblamaydi.
        foreach (var otp in verifiedCodes)
        {
            otp.VerifiedAt = null;
            _uow.OtpCodes.Update(otp);
        }

        await _uow.SaveChangesAsync(ct);
    }

    private static string GenerateCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString(new string('0', length));
    }
}
