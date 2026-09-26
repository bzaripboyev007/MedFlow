using MedFlow.Domain.Enums;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PaymentServices.Concrete;

public interface IPaymentService : IStatusGeneric
{
    /// <summary>Kurs uchun bitta yakuniy to'lovni hisoblab yaratadi (xizmat + dori + transport). Kurs 'Done' bo'lishi va oldin to'lov yaratilmagan bo'lishi shart.</summary>
    Task<PaymentDto?> CreateForCourseAsync(long courseId, PaymentMethod method, CancellationToken ct = default);
    Task<PaymentDto?> GetByCourseAsync(long courseId, CancellationToken ct = default);
    Task<List<PaymentDto>> ListByPatientAsync(long patientId, CancellationToken ct = default);
}
