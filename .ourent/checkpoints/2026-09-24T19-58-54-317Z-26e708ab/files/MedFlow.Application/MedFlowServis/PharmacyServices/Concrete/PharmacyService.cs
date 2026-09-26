using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PharmacyServices.Concrete;

public class PharmacyService : StatusGenericHandler, IPharmacyService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PharmacyService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<MedicineDto?> CreateAsync(CreateMedicineRequest request, CancellationToken ct = default)
    {
        var medicine = new Medicine
        {
            Name = request.Name,
            Unit = request.Unit,
            Price = request.Price,
            StockQty = request.StockQty,
            MinStockLevel = request.MinStockLevel,
            ExpiryDate = request.ExpiryDate
        };

        await _uow.Medicines.AddAsync(medicine, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<MedicineDto>(medicine);
    }

    public async Task<MedicineDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var medicine = await _uow.Medicines.GetByIdAsync(id, ct);
        if (medicine is null)
        {
            AddError($"{nameof(Medicine)} (Id={id}) topilmadi.");
            return null;
        }

        return _mapper.Map<MedicineDto>(medicine);
    }

    public async Task<List<MedicineDto>> ListAsync(CancellationToken ct = default)
    {
        var medicines = await _uow.Medicines.Query().OrderBy(m => m.Name).ToListAsync(ct);
        return _mapper.Map<List<MedicineDto>>(medicines);
    }

    public async Task<MedicineDto?> UpdateAsync(int id, UpdateMedicineRequest request, CancellationToken ct = default)
    {
        var medicine = await _uow.Medicines.GetByIdAsync(id, ct);
        if (medicine is null)
        {
            AddError($"{nameof(Medicine)} (Id={id}) topilmadi.");
            return null;
        }

        medicine.Name = request.Name;
        medicine.Unit = request.Unit;
        medicine.Price = request.Price;
        medicine.MinStockLevel = request.MinStockLevel;
        medicine.ExpiryDate = request.ExpiryDate;

        _uow.Medicines.Update(medicine);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<MedicineDto>(medicine);
    }

    public async Task<MedicineDto?> StockInAsync(int id, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0)
        {
            AddError("Miqdor 0 dan katta bo'lishi kerak.");
            return null;
        }

        var medicine = await _uow.Medicines.GetByIdAsync(id, ct);
        if (medicine is null)
        {
            AddError($"{nameof(Medicine)} (Id={id}) topilmadi.");
            return null;
        }

        medicine.StockQty += quantity;

        _uow.Medicines.Update(medicine);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<MedicineDto>(medicine);
    }

    public async Task<List<MedicineDto>> GetAlertsAsync(CancellationToken ct = default)
    {
        var expiryThreshold = DateTime.UtcNow.Date.AddDays(SchedulingConstants.ExpirySoonThresholdDays);

        var medicines = await _uow.Medicines.Query()
            .Where(m => m.StockQty <= m.MinStockLevel || m.ExpiryDate <= expiryThreshold)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

        return _mapper.Map<List<MedicineDto>>(medicines);
    }
}
