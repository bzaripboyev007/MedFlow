using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.WalletServices;

public class PurchaseWalletRequest
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public WalletSource Source { get; set; }
}
