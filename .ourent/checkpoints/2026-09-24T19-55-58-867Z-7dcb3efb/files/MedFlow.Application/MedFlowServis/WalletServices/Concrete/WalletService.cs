using AutoMapper;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.WalletServices.Concrete;

public class WalletService : StatusGenericHandler, IWalletService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public WalletService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<WalletEntryDto?> PurchaseAsync(long patientId, PurchaseWalletRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
        {
            AddError("Miqdor 0 dan katta bo'lishi kerak.");
            return null;
        }

        if (await _uow.Patients.GetByIdAsync(patientId, ct) is null)
        {
            AddError($"{nameof(Patient)} (Id={patientId}) topilmadi.");
            return null;
        }

        var medicine = await _uow.Medicines.GetByIdAsync(request.MedicineId, ct);
        if (medicine is null)
        {
            AddError($"{nameof(Medicine)} (Id={request.MedicineId}) topilmadi.");
            return null;
        }

        if (request.Source == WalletSource.Pharmacy)
        {
            if (medicine.StockQty < request.Quantity)
            {
                AddError("Omborda yetarli dori qoldig'i yo'q.");
                return null;
            }

            medicine.StockQty -= request.Quantity;
            _uow.Medicines.Update(medicine);
        }

        var entry = new PatientMedicineWallet
        {
            PatientId = patientId,
            MedicineId = medicine.Id,
            PurchasedQty = request.Quantity,
            RemainingQty = request.Quantity,
            Source = request.Source,
            PurchasedAt = DateTimeOffset.UtcNow
        };

        await _uow.WalletEntries.AddAsync(entry, ct);
        await _uow.SaveChangesAsync(ct);

        var dto = _mapper.Map<WalletEntryDto>(entry);
        dto.MedicineName = medicine.Name;
        return dto;
    }

    public async Task<List<WalletEntryDto>> GetWalletAsync(long patientId, CancellationToken ct = default)
    {
        var entries = await _uow.WalletEntries.Query()
            .Include(w => w.Medicine)
            .Where(w => w.PatientId == patientId)
            .OrderBy(w => w.PurchasedAt)
            .ToListAsync(ct);

        return _mapper.Map<List<WalletEntryDto>>(entries);
    }
}
