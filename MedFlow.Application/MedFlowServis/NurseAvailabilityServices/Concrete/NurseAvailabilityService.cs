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

        var busyStarts = await GetBusyStartsAsync(nurseId, ct);

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

    public async Task<List<(DateTimeOffset From, DateTimeOffset To)>> GetBusySlotsAsync(int nurseId, DateTime date, CancellationToken ct = default)
    {
        var duration = TimeSpan.FromMinutes(SchedulingConstants.DefaultSessionDurationMinutes);
        var dayStart = new DateTimeOffset(date.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        var busyStarts = await GetBusyStartsAsync(nurseId, ct, dayStart, dayEnd);

        return busyStarts
            .OrderBy(s => s)
            .Select(s => (From: s, To: s + duration))
            .ToList();
    }

    /// <summary>
    /// Hamshira vaqtini band qiladigan boshlanish nuqtalari: faol seanslar va faol bronlar.
    /// Ilgari faqat seanslar hisobga olinardi (X-8); bronlar "Yakunlandi"/"Bekor qilindi"
    /// bo'lmasa hamshira vaqtini band qiladi.
    /// </summary>
    private async Task<List<DateTimeOffset>> GetBusyStartsAsync(
        int nurseId,
        CancellationToken ct,
        DateTimeOffset? dayStart = null,
        DateTimeOffset? dayEnd = null)
    {
        var sessionsQuery = _uow.Sessions.Query()
            .Where(s => s.NurseId == nurseId
                        && (s.Status == SessionStatus.Pending || s.Status == SessionStatus.Arrived || s.Status == SessionStatus.InProgress));

        var bookingsQuery = _uow.Bookings.Query()
            .Where(b => b.NurseId == nurseId && BookingConstants.ActiveStatuses.Contains(b.Status));

        if (dayStart is { } from && dayEnd is { } to)
        {
            sessionsQuery = sessionsQuery.Where(s => s.ScheduledAt >= from && s.ScheduledAt < to);

            // Bron sana/vaqti satr ko'rinishida ("yyyy-MM-dd") saqlanadi — oralig'i:
            // [from.Date, to.Date) kuniga mos keladi.
            var day = from.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate == day);
        }

        var busyStarts = await sessionsQuery.Select(s => s.ScheduledAt).ToListAsync(ct);

        var activeBookings = await bookingsQuery
            .Select(b => new { b.BookingDate, b.BookingTime })
            .ToListAsync(ct);

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

        return busyStarts;
    }
}
