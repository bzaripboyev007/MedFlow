using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.SessionServices.Concrete;

public class SessionService : StatusGenericHandler, ISessionService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationSender _notifications;
    private readonly IMapper _mapper;

    public SessionService(IUnitOfWork uow, INotificationSender notifications, IMapper mapper)
    {
        _uow = uow;
        _notifications = notifications;
        _mapper = mapper;
    }

    public async Task<SessionDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var session = await LoadSessionAsync(id, ct);
        if (session is null)
        {
            AddError($"{nameof(Session)} (Id={id}) topilmadi.");
            return null;
        }

        return _mapper.Map<SessionDto>(session);
    }

    public async Task<List<SessionDto>> ListByCourseAsync(long courseId, CancellationToken ct = default)
    {
        var sessions = await _uow.Sessions.Query()
            .Include(s => s.Nurse)
            .Include(s => s.Course) // SessionDto.PatientId shu navigatsiyadan olinadi.
            .Where(s => s.CourseId == courseId)
            .OrderBy(s => s.SeqNo)
            .ToListAsync(ct);

        return _mapper.Map<List<SessionDto>>(sessions);
    }

    public async Task<SessionDto?> ArriveAsync(long sessionId, CancellationToken ct = default)
    {
        var session = await LoadSessionAsync(sessionId, ct);
        if (session is null)
        {
            AddError($"{nameof(Session)} (Id={sessionId}) topilmadi.");
            return null;
        }

        if (session.Status != SessionStatus.Pending)
        {
            AddError($"Seans '{session.Status}' holatida - 'Keldim' faqat 'Pending' holatda mumkin.");
            return null;
        }

        session.Status = SessionStatus.Arrived;
        session.ActualStartAt = DateTimeOffset.UtcNow;
        _uow.Sessions.Update(session);

        var course = await _uow.Courses.GetByIdAsync(session.CourseId, ct);
        if (course is { Status: CourseStatus.Pending })
        {
            course.Status = CourseStatus.Active;
            _uow.Courses.Update(course);
        }

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<SessionDto>(session);
    }

    public async Task<SessionDto?> CompleteAsync(long sessionId, string? notes, CancellationToken ct = default)
    {
        var session = await LoadSessionAsync(sessionId, ct);
        if (session is null)
        {
            AddError($"{nameof(Session)} (Id={sessionId}) topilmadi.");
            return null;
        }

        if (session.Status != SessionStatus.Arrived && session.Status != SessionStatus.InProgress)
        {
            AddError($"Seans '{session.Status}' holatida - avval 'Keldim' bosilishi kerak.");
            return null;
        }

        session.Status = SessionStatus.Done;
        session.ActualEndAt = DateTimeOffset.UtcNow;
        session.Notes = notes;
        _uow.Sessions.Update(session);

        var course = await _uow.Courses.GetByIdAsync(session.CourseId, ct);
        if (course is null)
        {
            AddError($"{nameof(TreatmentCourse)} (Id={session.CourseId}) topilmadi.");
            return null;
        }

        course.CompletedSessions += 1;
        if (course.CompletedSessions >= course.TotalSessions)
        {
            course.Status = CourseStatus.Done;
        }

        _uow.Courses.Update(course);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<SessionDto>(session);
    }

    public async Task<SessionDto?> MarkMissedAsync(long sessionId, CancellationToken ct = default)
    {
        var session = await LoadSessionAsync(sessionId, ct);
        if (session is null)
        {
            AddError($"{nameof(Session)} (Id={sessionId}) topilmadi.");
            return null;
        }

        if (session.Status != SessionStatus.Pending)
        {
            AddError($"Seans '{session.Status}' holatida - 'Missed' faqat 'Pending' holatdan mumkin.");
            return null;
        }

        session.Status = SessionStatus.Missed;
        _uow.Sessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        await RescheduleRemainingAsync(session.CourseId, session.SeqNo, ct);

        return _mapper.Map<SessionDto>(session);
    }

    public async Task<List<MedicineIssueDto>> IssueMedicineAsync(long sessionId, int medicineId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0)
        {
            AddError("Miqdor 0 dan katta bo'lishi kerak.");
            return new List<MedicineIssueDto>();
        }

        var session = await LoadSessionAsync(sessionId, ct);
        if (session is null)
        {
            AddError($"{nameof(Session)} (Id={sessionId}) topilmadi.");
            return new List<MedicineIssueDto>();
        }

        var medicine = await _uow.Medicines.GetByIdAsync(medicineId, ct);
        if (medicine is null)
        {
            AddError($"{nameof(Medicine)} (Id={medicineId}) topilmadi.");
            return new List<MedicineIssueDto>();
        }

        var patientId = session.Course.PatientId;

        var walletEntries = await _uow.WalletEntries.Query()
            .Where(w => w.PatientId == patientId && w.MedicineId == medicineId && w.RemainingQty > 0)
            .OrderBy(w => w.PurchasedAt)
            .ToListAsync(ct);

        var remaining = quantity;
        var fromWallet = 0;
        foreach (var entry in walletEntries)
        {
            if (remaining <= 0) break;
            var take = Math.Min(entry.RemainingQty, remaining);
            entry.RemainingQty -= take;
            remaining -= take;
            fromWallet += take;
            _uow.WalletEntries.Update(entry);
        }

        var results = new List<MedicineIssue>();

        if (fromWallet > 0)
        {
            var walletIssue = new MedicineIssue
            {
                MedicineId = medicineId,
                PatientId = patientId,
                NurseId = session.NurseId,
                SessionId = session.Id,
                Quantity = fromWallet,
                Source = IssueSource.PatientWallet,
                PriceAtIssue = 0, // xarid vaqtida allaqachon to'langan
                IssuedAt = DateTimeOffset.UtcNow
            };
            await _uow.MedicineIssues.AddAsync(walletIssue, ct);
            results.Add(walletIssue);
        }

        if (remaining > 0)
        {
            if (medicine.StockQty < remaining)
            {
                AddError("Bemor shkafida yetarli dori yo'q va omborda ham yetarli qoldiq yo'q.");
                return new List<MedicineIssueDto>();
            }

            medicine.StockQty -= remaining;
            _uow.Medicines.Update(medicine);

            var pharmacyIssue = new MedicineIssue
            {
                MedicineId = medicineId,
                PatientId = patientId,
                NurseId = session.NurseId,
                SessionId = session.Id,
                Quantity = remaining,
                Source = IssueSource.Pharmacy,
                PriceAtIssue = medicine.Price * remaining,
                IssuedAt = DateTimeOffset.UtcNow
            };
            await _uow.MedicineIssues.AddAsync(pharmacyIssue, ct);
            results.Add(pharmacyIssue);
        }

        await _uow.SaveChangesAsync(ct);

        var dtos = _mapper.Map<List<MedicineIssueDto>>(results);
        dtos.ForEach(d => d.MedicineName = medicine.Name);
        return dtos;
    }

    public async Task<int> SweepMissedSessionsAsync(CancellationToken ct = default)
    {
        var threshold = DateTimeOffset.UtcNow.AddMinutes(-SchedulingConstants.MissedSessionGraceMinutes);

        var overdueSessionIds = await _uow.Sessions.Query()
            .Where(s => s.Status == SessionStatus.Pending && s.ScheduledAt < threshold)
            .OrderBy(s => s.ScheduledAt)
            .Select(s => s.Id)
            .ToListAsync(ct);

        foreach (var id in overdueSessionIds)
        {
            await MarkMissedAsync(id, ct);
        }

        return overdueSessionIds.Count;
    }

    private async Task RescheduleRemainingAsync(long courseId, int fromSeqNo, CancellationToken ct)
    {
        var remaining = await _uow.Sessions.Query()
            .Where(s => s.CourseId == courseId && s.SeqNo > fromSeqNo && s.Status == SessionStatus.Pending)
            .OrderBy(s => s.SeqNo)
            .ToListAsync(ct);

        if (remaining.Count == 0) return;

        foreach (var s in remaining)
        {
            s.ScheduledAt = s.ScheduledAt.AddDays(SchedulingConstants.RescheduleShiftDays);
            _uow.Sessions.Update(s);
        }

        await _uow.SaveChangesAsync(ct);

        var course = await _uow.Courses.GetByIdAsync(courseId, ct);
        if (course is not null)
        {
            await _notifications.NotifyPatientAsync(
                course.PatientId,
                $"Bitta seans o'tkazib yuborildi. Qolgan {remaining.Count} ta seans {SchedulingConstants.RescheduleShiftDays} kunga surildi.",
                ct);
        }
    }

    private async Task<Session?> LoadSessionAsync(long id, CancellationToken ct)
    {
        return await _uow.Sessions.Query()
            .Include(s => s.Nurse)
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }
}
