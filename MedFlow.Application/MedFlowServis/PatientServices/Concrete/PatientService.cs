using System.Globalization;
using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PatientServices.Concrete;

public class PatientService : StatusGenericHandler, IPatientService
{
    private static readonly string[] VisitedBookingStatuses =
    {
        BookingConstants.StatusArrived, BookingConstants.StatusInProgress, BookingConstants.StatusCompleted
    };

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

        var dto = _mapper.Map<PatientDto>(patient);
        await FillLastVisitDatesAsync(new List<PatientDto> { dto }, ct);
        return dto;
    }

    public async Task<List<PatientDto>> ListAsync(long? parentId, CancellationToken ct = default)
    {
        var query = _uow.Patients.Query();
        if (parentId is { } pid)
        {
            query = query.Where(p => p.ParentId == pid);
        }

        var patients = await query.OrderBy(p => p.FullName).ToListAsync(ct);
        var dtos = _mapper.Map<List<PatientDto>>(patients);
        await FillLastVisitDatesAsync(dtos, ct);
        return dtos;
    }

    /// <summary>
    /// Har bir bemor uchun muolaja boshlangan payt va oxirgi tashrif sanasini ikki manbadan topadi:
    /// 1) yakunlangan ('Done') kurs seanslari - bemorga to'g'ridan-to'g'ri bog'langan;
    /// 2) bemor haqiqatda kelgan bronlar - bronda PatientId yo'q, shuning uchun telefon raqami bo'yicha.
    /// </summary>
    private async Task FillLastVisitDatesAsync(List<PatientDto> patients, CancellationToken ct)
    {
        if (patients.Count == 0)
        {
            return;
        }

        var ids = patients.Select(p => p.Id).ToList();

        var sessionVisits = await _uow.Sessions.Query()
            .Where(s => s.Status == SessionStatus.Done && ids.Contains(s.Course.PatientId))
            .GroupBy(s => s.Course.PatientId)
            .Select(g => new { PatientId = g.Key, Last = g.Max(s => s.ActualEndAt ?? s.ScheduledAt) })
            .ToDictionaryAsync(x => x.PatientId, x => x.Last, ct);

        // Muolaja boshlangan payt: hamshira birinchi marta "Keldim" bosgan seans (ActualStartAt).
        var sessionStarts = await _uow.Sessions.Query()
            .Where(s => s.ActualStartAt != null && ids.Contains(s.Course.PatientId))
            .GroupBy(s => s.Course.PatientId)
            .Select(g => new { PatientId = g.Key, First = g.Min(s => s.ActualStartAt) })
            .ToDictionaryAsync(x => x.PatientId, x => x.First, ct);

        // "Yangi" (hali kelmagan) va "Bekor qilindi" bronlar tashrif hisoblanmaydi.
        // BOOKING_DATE "yyyy-MM-dd", BOOKING_TIME "HH:mm" matn bo'lgani uchun Max()/Min()
        // xronologik tartibga mos keladi.
        var bookingRows = await _uow.Bookings.Query()
            .Where(b => VisitedBookingStatuses.Contains(b.Status))
            .GroupBy(b => b.Phone)
            .Select(g => new
            {
                Phone = g.Key,
                Last = g.Max(b => b.BookingDate),
                First = g.Min(b => b.BookingDate + " " + b.BookingTime)
            })
            .ToListAsync(ct);

        // Bron telefoni foydalanuvchi yozganidek saqlanadi - solishtirishdan oldin kanonik ko'rinishga keltiramiz.
        var bookingLast = new Dictionary<string, DateTime>();
        var bookingFirst = new Dictionary<string, DateTime>();
        foreach (var row in bookingRows)
        {
            var phone = PhoneNumberNormalizer.Normalize(row.Phone);

            if (DateTime.TryParseExact(row.Last, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastDate)
                && (!bookingLast.TryGetValue(phone, out var knownLast) || lastDate > knownLast))
            {
                bookingLast[phone] = lastDate;
            }

            if (DateTime.TryParseExact(row.First, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var firstAt)
                && (!bookingFirst.TryGetValue(phone, out var knownFirst) || firstAt < knownFirst))
            {
                bookingFirst[phone] = firstAt;
            }
        }

        foreach (var patient in patients)
        {
            DateTime? last = sessionVisits.TryGetValue(patient.Id, out var sessionAt)
                ? sessionAt.ToLocalTime().Date
                : null;

            DateTime? first = sessionStarts.TryGetValue(patient.Id, out var startedAt) && startedAt is { } started
                ? started.ToLocalTime().DateTime
                : null;

            var phone = PhoneNumberNormalizer.Normalize(patient.Phone);
            if (phone.Length > 0)
            {
                if (bookingLast.TryGetValue(phone, out var bookingDate) && (last is null || bookingDate > last))
                {
                    last = bookingDate;
                }

                if (bookingFirst.TryGetValue(phone, out var bookingAt) && (first is null || bookingAt < first))
                {
                    first = bookingAt;
                }
            }

            patient.LastVisitDate = last;
            patient.TreatmentStartedAt = first?.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }
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

        var items = _mapper.Map<List<PatientDto>>(patients);
        await FillLastVisitDatesAsync(items, ct);

        return new PagedResultDto<PatientDto>
        {
            Items = items,
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
