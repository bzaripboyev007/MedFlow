using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.WalletServices.Concrete;

public interface IWalletService : IStatusGeneric
{
    Task<WalletEntryDto?> PurchaseAsync(long patientId, PurchaseWalletRequest request, CancellationToken ct = default);
    Task<List<WalletEntryDto>> GetWalletAsync(long patientId, CancellationToken ct = default);
}
