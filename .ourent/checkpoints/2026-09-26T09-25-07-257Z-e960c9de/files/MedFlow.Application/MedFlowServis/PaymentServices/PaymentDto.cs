using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.PaymentServices;

public class PaymentDto
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public long CourseId { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal MedicineAmount { get; set; }
    public decimal TransportAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTimeOffset PaidAt { get; set; }
}
