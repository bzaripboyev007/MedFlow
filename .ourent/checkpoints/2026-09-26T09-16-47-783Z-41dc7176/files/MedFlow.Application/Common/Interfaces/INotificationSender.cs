namespace MedFlow.Application.Common.Interfaces;

/// <summary>
/// Bemor/hamshiraga eslatma yuborish abstraksiyasi. Hozircha faqat log yozadigan implementatsiya bor -
/// Telegram bot bosqichi qo'shilganda shu interfeys orqasida haqiqiy yuborish ulanadi.
/// </summary>
public interface INotificationSender
{
    Task NotifyPatientAsync(long patientId, string message, CancellationToken ct = default);
}
