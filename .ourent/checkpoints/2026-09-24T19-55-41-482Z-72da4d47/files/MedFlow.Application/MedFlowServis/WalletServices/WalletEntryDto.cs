using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.WalletServices;

public class WalletEntryDto
{
    public long Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public int PurchasedQty { get; set; }
    public int RemainingQty { get; set; }
    public WalletSource Source { get; set; }
    public DateTimeOffset PurchasedAt { get; set; }
}
