using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PatientServices.Concrete;

public class PatientService : StatusGenericHandler, IPatientService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public PatientService(IUnitOfWork uow, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<PatientDto?> CreateAsync(CreatePatientRequest request, CancellationToken ct = default)
    {
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.Phone);
        if (!PhoneNumberNormalizer.IsValidUzbek(phoneNumber))
        {
            AddError(PhoneNumberNormalizer.InvalidMessage);
            return null;
        }

        if (request.ParentId is { } parentId && await _uow.Patients.GetByIdAsync(parentId, ct) is null)
        {
            AddError($"{nameof(Patient)} (Id={parentId}) topilmadi.");
            return null;
        }

        var patient = new Patient
        {
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            Phone = phoneNumber,
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

    public async Task<PagedResultDto<PatientDto>> SortFilterAsync(PatientSortFilterRequest request, CancellationToken ct = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _uow.Patients.Query();

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var pattern = search.ToLower();
            query = query.Where(p => p.FullName.ToLower().Contains(pattern));
        }

        if (request.IsChild is { } isChild)
        {
            query = query.Where(p => p.IsChild == isChild);
        }

        query = (request.SortBy, request.SortDesc) switch
        {
            (PatientSortField.BirthDate, false) => query.OrderBy(p => p.BirthDate).ThenBy(p => p.Id),
            (PatientSortField.BirthDate, true) => query.OrderByDescending(p => p.BirthDate).ThenBy(p => p.Id),
            (PatientSortField.CreatedAt, false) => query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
            (PatientSortField.CreatedAt, true) => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
            (_, true) => query.OrderByDescending(p => p.FullName).ThenBy(p => p.Id),
            _ => query.OrderBy(p => p.FullName).ThenBy(p => p.Id)
        };

        var totalCount = await query.CountAsync(ct);
        var patients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResultDto<PatientDto>
        {
            Items = _mapper.Map<List<PatientDto>>(patients),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PatientDto?> UpdateAsync(long id, UpdatePatientRequest request, CancellationToken ct = default)
    {
        var patient = await _uow.Patients.GetByIdAsync(id, ct);
        if (patient is null)
        {
            AddError($"{nameof(Patient)} (Id={id}) topilmadi.");
            return null;
        }

        var phoneNumber = PhoneNumberNormalizer.Normalize(request.Phone);
        if (!PhoneNumberNormalizer.IsValidUzbek(phoneNumber))
        {
            AddError(PhoneNumberNormalizer.InvalidMessage);
            return null;
        }

        patient.FullName = request.FullName;
        patient.Phone = phoneNumber;
        patient.Allergies = request.Allergies;

        _uow.Patients.Update(patient);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<bool> ChangePasswordAsync(long patientId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < AuthConstants.MinPasswordLength)
        {
            AddError($"Parol kamida {AuthConstants.MinPasswordLength} belgidan iborat bo'lishi kerak.");
            return false;
        }

        if (await _uow.Patients.GetByIdAsync(patientId, ct) is null)
        {
            AddError($"{nameof(Patient)} (Id={patientId}) topilmadi.");
            return false;
        }

        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.LinkedPatientId == patientId, ct);
        if (user is null)
        {
            // Masalan, ota-ona orqali qo'shilgan bola profilining o'z login hisobi bo'lmaydi.
            AddError("Bu bemorning tizimga kirish hisobi yo'q.");
            return false;
        }

        await UserPasswordReset.ApplyAsync(_uow, _passwordHasher, user, newPassword, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsSelfOrChildAsync(long callerPatientId, long targetPatientId, CancellationToken ct = default)
    {
        if (callerPatientId == targetPatientId) return true;

        var target = await _uow.Patients.GetByIdAsync(targetPatientId, ct);
        return target is not null && target.ParentId == callerPatientId;
    }
}
