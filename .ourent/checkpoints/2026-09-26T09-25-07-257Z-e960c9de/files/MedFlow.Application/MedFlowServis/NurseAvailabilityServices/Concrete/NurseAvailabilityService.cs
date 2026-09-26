using System.Globalization;
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

        // Bronlar ham hamshira vaqtini band qiladi (uyga chaqirish/klinika). Ilgari faqat
        // seanslar hisobga olinardi, natijada bir vaqtga ikki mijoz yozilardi (X-8).
        // "Yakunlandi"/"Bekor qilindi" bronlar band hisoblanmaydi.
        var activeBookings = await _uow.Bookings.Query()
            .Where(b => b.NurseId == nurseId && BookingConstants.ActiveStatuses.Contains(b.Status))
            .Select(b => new { b.BookingDate, b.BookingTime })
            .ToListAsync(ct);

        var busyStarts = new List<DateTimeOffset>(busySessions);
        foreach (var booking in activeBookings)
        {
            // Bron sana/vaqti matn ko'rinishida saqlanadi: "yyyy-MM-dd" va "HH:mm".
            // Buzilgan (eski) yozuvlar seans rejalashtirishni bloklamasligi uchun o'tkazib yuboriladi.
            if (DateOnly.TryParseExact(booking.BookingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                && TimeOnly.TryParseExact(booking.BookingTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                busyStarts.Add(new DateTimeOffset(date.ToDateTime(time), TimeSpan.Zero));
            }
        }

        foreach (var time in times)
        {
            var newFrom = time;
            var newTo = time + duration;

            foreach (var busyFrom in busyStarts)
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
