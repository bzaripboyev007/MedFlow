using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PharmacyServices.Concrete;

public interface IPharmacyService : IStatusGeneric
{
    Task<MedicineDto?> CreateAsync(CreateMedicineRequest request, CancellationToken ct = default);
    Task<MedicineDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<MedicineDto>> ListAsync(CancellationToken ct = default);
    Task<MedicineDto?> UpdateAsync(int id, UpdateMedicineRequest request, CancellationToken ct = default);
    Task<MedicineDto?> StockInAsync(int id, int quantity, CancellationToken ct = default);
    Task<List<MedicineDto>> GetAlertsAsync(CancellationToken ct = default);
}
