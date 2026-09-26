using AutoMapper;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PrescriptionServices.Concrete;

public class PrescriptionService : StatusGenericHandler, IPrescriptionService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _fileStorage;
    private readonly IMapper _mapper;

    public PrescriptionService(IUnitOfWork uow, IFileStorageService fileStorage, IMapper mapper)
    {
        _uow = uow;
        _fileStorage = fileStorage;
        _mapper = mapper;
    }

    public async Task<PrescriptionDto?> UploadAsync(long patientId, Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        if (await _uow.Patients.GetByIdAsync(patientId, ct) is null)
        {
            AddError($"{nameof(Patient)} (Id={patientId}) topilmadi.");
            return null;
        }

        var storageKey = await _fileStorage.SaveAsync(content, fileName, contentType, ct);

        var prescription = new Prescription
        {
            PatientId = patientId,
            FileUrl = _fileStorage.GetUrl(storageKey),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.Prescriptions.AddAsync(prescription, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PrescriptionDto>(prescription);
    }

    public async Task<PrescriptionDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.GetByIdAsync(id, ct);
        if (prescription is null)
        {
            AddError($"{nameof(Prescription)} (Id={id}) topilmadi.");
            return null;
        }

        return _mapper.Map<PrescriptionDto>(prescription);
    }

    public async Task<List<PrescriptionDto>> ListByPatientAsync(long patientId, CancellationToken ct = default)
    {
        var prescriptions = await _uow.Prescriptions.Query()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return _mapper.Map<List<PrescriptionDto>>(prescriptions);
    }

    public async Task<PrescriptionDto?> VerifyAsync(long id, int nurseId, CancellationToken ct = default)
    {
        var prescription = await _uow.Prescriptions.GetByIdAsync(id, ct);
        if (prescription is null)
        {
            AddError($"{nameof(Prescription)} (Id={id}) topilmadi.");
            return null;
        }

        if (await _uow.Nurses.GetByIdAsync(nurseId, ct) is null)
        {
            AddError($"{nameof(Nurse)} (Id={nurseId}) topilmadi.");
            return null;
        }

        prescription.VerifiedByNurseId = nurseId;
        prescription.VerifiedAt = DateTimeOffset.UtcNow;

        _uow.Prescriptions.Update(prescription);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PrescriptionDto>(prescription);
    }
}
