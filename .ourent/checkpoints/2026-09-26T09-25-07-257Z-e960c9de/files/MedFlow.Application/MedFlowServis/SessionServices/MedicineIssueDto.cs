using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.SessionServices;

public class MedicineIssueDto
{
    public long Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public int Quantity { get; set; }
    public IssueSource Source { get; set; }
    public decimal PriceAtIssue { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
}
