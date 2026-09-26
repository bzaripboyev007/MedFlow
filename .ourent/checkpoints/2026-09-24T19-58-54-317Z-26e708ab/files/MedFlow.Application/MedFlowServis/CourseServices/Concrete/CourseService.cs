using System.Globalization;
using AutoMapper;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Application.MedFlowServis.NurseAssignmentServices.Concrete;
using MedFlow.Application.MedFlowServis.WalletServices;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.CourseServices.Concrete;

public class CourseService : StatusGenericHandler, ICourseService
{
    private readonly IUnitOfWork _uow;
    private readonly INurseAssignmentService _assignment;
    private readonly IMapper _mapper;

    public CourseService(IUnitOfWork uow, INurseAssignmentService assignment, IMapper mapper)
    {
        _uow = uow;
        _assignment = assignment;
        _mapper = mapper;
    }

    public async Task<CourseDto?> CreateAsync(CreateCourseRequest request, CancellationToken ct = default)
    {
        if (request.DurationDays <= 0)
        {
            AddError("Kurs davomiyligi 0 dan katta bo'lishi kerak.");
            return null;
        }

        if (request.TimesPerDay <= 0)
        {
            AddError("Kunlik chastota 0 dan katta bo'lishi kerak.");
            return null;
        }

        if (request.PreferredTimes.Count != request.TimesPerDay)
        {
            AddError("Qulay vaqtlar soni kunlik chastotaga (TimesPerDay) teng bo'lishi kerak.");
            return null;
        }

        var patient = await _uow.Patients.GetByIdAsync(request.PatientId, ct);
        if (patient is null)
        {
            AddError($"{nameof(Patient)} (Id={request.PatientId}) topilmadi.");
            return null;
        }

        var prescription = await _uow.Prescriptions.GetByIdAsync(request.PrescriptionId, ct);
        if (prescription is null)
        {
            AddError($"{nameof(Prescription)} (Id={request.PrescriptionId}) topilmadi.");
            return null;
        }

        if (prescription.PatientId != patient.Id)
        {
            AddError("Retsept shu bemorga tegishli emas.");
            return null;
        }

        var serviceType = await _uow.ServiceTypes.GetByIdAsync(request.ServiceTypeId, ct);
        if (serviceType is null)
        {
            AddError($"{nameof(ServiceType)} (Id={request.ServiceTypeId}) topilmadi.");
            return null;
        }

        TimeSpan[] times;
        try
        {
            times = request.PreferredTimes.Select(ParseTime).OrderBy(t => t).ToArray();
        }
        catch (FormatException ex)
        {
            AddError(ex.Message);
            return null;
        }

        var sessionTimes = new List<DateTimeOffset>();
        for (var day = 0; day < request.DurationDays; day++)
        {
            foreach (var time in times)
            {
                var dt = request.StartDate.Date.AddDays(day).Add(time);
                sessionTimes.Add(new DateTimeOffset(dt, TimeSpan.Zero));
            }
        }

        var nurse = await _assignment.AssignAsync(
            patient, request.DesiredNurseGender, sessionTimes, request.AssignmentMode, request.RequestedNurseId, ct);
        if (!_assignment.IsValid)
        {
            CombineStatuses(_assignment);
            return null;
        }

        var course = new TreatmentCourse
        {
            PatientId = patient.Id,
            PrescriptionId = prescription.Id,
            PrimaryNurseId = nurse!.Id,
            ServiceTypeId = serviceType.Id,
            LocationType = request.LocationType,
            Address = request.Address,
            DurationDays = request.DurationDays,
            TimesPerDay = request.TimesPerDay,
            PreferredTimesMinutes = times.Select(t => (int)t.TotalMinutes).ToArray(),
            StartDate = request.StartDate.Date,
            TotalSessions = sessionTimes.Count,
            CompletedSessions = 0,
            Status = CourseStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        for (var i = 0; i < sessionTimes.Count; i++)
        {
            course.Sessions.Add(new Session
            {
                SeqNo = i + 1,
                ScheduledAt = sessionTimes[i],
                NurseId = nurse.Id,
                Status = SessionStatus.Pending
            });
        }

        await _uow.Courses.AddAsync(course, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(course.Id, ct);
    }

    public async Task<CourseDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var course = await _uow.Courses.Query()
            .Include(c => c.Patient)
            .Include(c => c.PrimaryNurse)
            .Include(c => c.ServiceType)
            .Include(c => c.Sessions).ThenInclude(s => s.Nurse)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
        {
            AddError($"{nameof(TreatmentCourse)} (Id={id}) topilmadi.");
            return null;
        }

        return ToDto(course);
    }

    public async Task<List<CourseSummaryDto>> ListAsync(long? patientId, int? nurseId, CourseStatus? status, CancellationToken ct = default)
    {
        var query = _uow.Courses.Query()
            .Include(c => c.Patient)
            .Include(c => c.PrimaryNurse)
            .AsQueryable();

        if (patientId is { } pid) query = query.Where(c => c.PatientId == pid);
        if (nurseId is { } nid) query = query.Where(c => c.PrimaryNurseId == nid);
        if (status is { } st) query = query.Where(c => c.Status == st);

        var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);

        return _mapper.Map<List<CourseSummaryDto>>(courses);
    }

    public async Task<CourseDto?> UpdateStatusAsync(long id, CourseStatus status, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(id, ct);
        if (course is null)
        {
            AddError($"{nameof(TreatmentCourse)} (Id={id}) topilmadi.");
            return null;
        }

        course.Status = status;
        _uow.Courses.Update(course);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<CourseWalletSummaryDto?> GetWalletSummaryAsync(long courseId, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            AddError($"{nameof(TreatmentCourse)} (Id={courseId}) topilmadi.");
            return null;
        }

        var entries = await _uow.WalletEntries.Query()
            .Include(w => w.Medicine)
            .Where(w => w.PatientId == course.PatientId)
            .OrderBy(w => w.PurchasedAt)
            .ToListAsync(ct);

        var dtos = _mapper.Map<List<WalletEntryDto>>(entries);

        return new CourseWalletSummaryDto { Entries = dtos };
    }

    private static TimeSpan ParseTime(string raw)
    {
        if (!TimeOnly.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var time))
        {
            throw new FormatException($"'{raw}' vaqt formati noto'g'ri, 'HH:mm' formatida bo'lishi kerak.");
        }

        return time.ToTimeSpan();
    }

    private CourseDto ToDto(TreatmentCourse c) => _mapper.Map<CourseDto>(c);
}
