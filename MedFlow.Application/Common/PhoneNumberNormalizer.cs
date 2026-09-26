namespace MedFlow.Application.Common;

/// <summary>
/// Telefon raqamlarini yagona E.164 ko'rinishga ("+998XXXXXXXXX") keltiruvchi yordamchi.
/// Ilgari Telegram bot raqamni "+" bilan saqlar, OTP/auth esa faqat <c>Trim()</c> qilib
/// qidirardi — natijada OTP, ro'yxatdan o'tish va parol tiklash oqimi umuman ishlamasdi.
/// Endi barcha yozish/qidirish joylarida shu yordamchi ishlatiladi.
/// </summary>
public static class PhoneNumberNormalizer
{
    /// <summary>Kanonik ko'rinish: bo'shliq/qavs/tirelar tozalanadi, "+" qo'shiladi: "+998901234567".</summary>
    public static string Normalize(string? phoneNumber)
    {
        var digits = DigitsOnly(phoneNumber);
        if (digits.Length == 0) return string.Empty;

        // 9 xonali raqam (998 prefiksisiz) kiritilsa, O'zbekiston kodi qo'shiladi.
        return digits.Length == 9 ? $"+998{digits}" : $"+{digits}";
    }

    /// <summary>Faqat raqamlar: "998901234567" (bazadagi eski, "+"siz yozuvlar uchun).</summary>
    public static string DigitsOnly(string? phoneNumber)
        => new((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>
    /// Bazadagi ustun bilan solishtirish uchun mumkin bo'lgan ko'rinishlar: kanonik
    /// "+998901234567" va eski "+"siz "998901234567". Shu tufayli normalizatsiyadan
    /// oldin saqlangan yozuvlar ham topilaveradi (ma'lumot migratsiyasisiz moslik).
    /// </summary>
    public static string[] StorageForms(string? phoneNumber)
    {
        var normalized = Normalize(phoneNumber);
        var digits = DigitsOnly(phoneNumber);

        return normalized == digits ? new[] { normalized } : new[] { normalized, digits };
    }
}
