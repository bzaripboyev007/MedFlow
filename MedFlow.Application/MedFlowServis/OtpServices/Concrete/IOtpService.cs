using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.OtpServices.Concrete;

public interface IOtpService : IStatusGeneric
{
    /// <summary>Telegram botga ulangan raqamga bir martalik tasdiqlash kodini yuboradi.</summary>
    Task<bool> SendOtpAsync(SendOtpRequest request, CancellationToken ct = default);

    /// <summary>Kodni tekshiradi. To'g'ri bo'lsa kodni "ishlatilgan" deb belgilab, tasdiqlangan holatni saqlaydi.</summary>
    Task<bool> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default);

    /// <summary>Berilgan raqam so'nggi bir necha daqiqada muvaffaqiyatli OTP orqali tasdiqlanganini tekshiradi.</summary>
    Task<bool> IsRecentlyVerifiedAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>Tasdiqlangan holatni "sarflaydi": bitta tasdiqlash faqat bitta amal (ro'yxatdan o'tish yoki parol tiklash) uchun yaraydi.</summary>
    Task ConsumeVerificationAsync(string phoneNumber, CancellationToken ct = default);
}
