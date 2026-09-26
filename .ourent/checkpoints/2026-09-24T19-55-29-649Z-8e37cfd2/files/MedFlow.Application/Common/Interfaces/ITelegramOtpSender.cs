namespace MedFlow.Application.Common.Interfaces;

/// <summary>Telegram Bot API orqali xabar yuborish uchun abstraksiya.</summary>
public interface ITelegramOtpSender
{
    /// <summary>Berilgan chat'ga matnli xabar yuboradi. Muvaffaqiyatli bo'lsa true qaytaradi.</summary>
    Task<bool> SendMessageAsync(long chatId, string text, CancellationToken ct = default);
}
