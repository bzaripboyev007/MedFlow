using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;

public class NurseAvailabilityService : INurseAvailabilityService
{
    private readonly IUnitOfWork _uow;

    public NurseAvailabilityService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> IsAvailableForAllAsync(int nurseId, IEnumerable<DateTimeOffset> times, CancellationToken ct = default)
    {
        var duration = TimeSpan.FromMinutes(SchedulingConstants.DefaultSessionDurationMinutes);

        var busySessions = await _uow.Sessions.Query()
            .Where(s => s.NurseId == nurseId
                        && (s.Status == SessionStatus.Pending || s.Status == SessionStatus.Arrived || s.Status == SessionStatus.InProgress))
            .Select(s => s.ScheduledAt)
            .ToListAsync(ct);

        foreach (var time in times)
        {
            var newFrom = time;
            var newTo = time + duration;

            foreach (var busyFrom in busySessions)
            {
                var busyTo = busyFrom + duration;
                var overlaps = newFrom < busyTo && busyFrom < newTo;
                if (overlaps)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
