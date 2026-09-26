using MedFlow.Application.Common.Interfaces;
using MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.NurseAssignmentServices.Concrete;

public class NurseAssignmentService : StatusGenericHandler, INurseAssignmentService
{
    private readonly IUnitOfWork _uow;
    private readonly INurseAvailabilityService _availability;

    public NurseAssignmentService(IUnitOfWork uow, INurseAvailabilityService availability)
    {
        _uow = uow;
        _availability = availability;
    }

    public async Task<Nurse?> AssignAsync(
        Patient patient,
        Gender? desiredGender,
        IReadOnlyList<DateTimeOffset> sessionTimes,
        AssignmentMode mode,
        int? requestedNurseId,
        CancellationToken ct = default)
    {
        var requiredType = patient.IsChild ? NurseType.Pediatric : NurseType.Adult;

        if (!patient.IsChild && desiredGender is null)
        {
            AddError("Kattalar bemori uchun hamshira jinsini tanlash majburiy.");
            return null;
        }

        var candidatesQuery = _uow.Nurses.Query()
            .Where(n => n.IsActive && n.Type == requiredType);

        if (!patient.IsChild)
        {
            candidatesQuery = candidatesQuery.Where(n => n.Gender == desiredGender);
        }

        var candidates = await candidatesQuery.ToListAsync(ct);

        if (mode == AssignmentMode.Manual)
        {
            if (requestedNurseId is null)
            {
                AddError("'O'zim tanlayman' rejimida hamshira Id si berilishi shart.");
                return null;
            }

            var chosen = candidates.FirstOrDefault(n => n.Id == requestedNurseId);
            if (chosen is null)
            {
                AddError("Tanlangan hamshira bemor turi/jinsiga mos kelmaydi yoki faol emas.");
                return null;
            }

            var isFree = await _availability.IsAvailableForAllAsync(chosen.Id, sessionTimes, ct);
            if (!isFree)
            {
                AddError("Tanlangan hamshira ko'rsatilgan seans vaqtlarida band.");
                return null;
            }

            return chosen;
        }

        // Auto: faqat barcha seans vaqtlarida bo'sh nomzodlar orasidan tanlanadi.
        var freeCandidates = new List<Nurse>();
        foreach (var candidate in candidates)
        {
            if (await _availability.IsAvailableForAllAsync(candidate.Id, sessionTimes, ct))
            {
                freeCandidates.Add(candidate);
            }
        }

        if (freeCandidates.Count == 0)
        {
            AddError("Mos va bo'sh hamshira topilmadi.");
            return null;
        }

        var previousNurseIds = (await _uow.Courses.Query()
                .Where(c => c.PatientId == patient.Id)
                .Select(c => c.PrimaryNurseId)
                .ToListAsync(ct))
            .ToHashSet();

        var workloads = new Dictionary<int, int>();
        foreach (var candidate in freeCandidates)
        {
            workloads[candidate.Id] = await _uow.Courses.Query()
                .CountAsync(c => c.PrimaryNurseId == candidate.Id
                                  && (c.Status == CourseStatus.Active || c.Status == CourseStatus.Pending), ct);
        }

        var best = freeCandidates
            .OrderByDescending(n => previousNurseIds.Contains(n.Id)) // oldingi tanishlik ustunligi
            .ThenBy(n => workloads[n.Id]) // eng kam yuklamali
            .First();

        return best;
    }
}
