using AutoMapper;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PatientServices.Concrete;

public class PatientService : StatusGenericHandler, IPatientService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PatientService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PatientDto?> CreateAsync(CreatePatientRequest request, CancellationToken ct = default)
    {
        if (request.ParentId is { } parentId && await _uow.Patients.GetByIdAsync(parentId, ct) is null)
        {
            AddError($"{nameof(Patient)} (Id={parentId}) topilmadi.");
            return null;
        }

        var patient = new Patient
        {
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            Phone = request.Phone,
            ParentId = request.ParentId,
            IsChild = request.IsChild,
            Allergies = request.Allergies,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.Patients.AddAsync(patient, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<PatientDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(id, ct);
        if (patient is null)
        {
            AddError($"{nameof(Patient)} (Id={id}) topilmadi.");
            return null;
        }

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<List<PatientDto>> ListAsync(long? parentId, CancellationToken ct = default)
    {
        var query = _uow.Patients.Query();
        if (parentId is { } pid)
        {
            query = query.Where(p => p.ParentId == pid);
        }

        var patients = await query.OrderBy(p => p.FullName).ToListAsync(ct);
        return _mapper.Map<List<PatientDto>>(patients);
    }

    public async Task<PatientDto?> UpdateAsync(long id, UpdatePatientRequest request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(id, ct);
        if (patient is null)
        {
            AddError($"{nameof(Patient)} (Id={id}) topilmadi.");
            return null;
        }

        patient.FullName = request.FullName;
        patient.Phone = request.Phone;
        patient.Allergies = request.Allergies;

        _uow.Patients.Update(patient);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<bool> IsSelfOrChildAsync(long callerPatientId, long targetPatientId, CancellationToken ct = default)
    {
        if (callerPatientId == targetPatientId) return true;

        var target = await _uow.Patients.GetByIdAsync(targetPatientId, ct);
        return target is not null && target.ParentId == callerPatientId;
    }
}
