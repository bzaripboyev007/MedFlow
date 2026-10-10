using System.Globalization;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.DashboardServices.Concrete;

public class DashboardService : StatusGenericHandler, IDashboardService
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<WeeklyStatDto>?> GetWeeklyStatsAsync(string? date, CancellationToken ct = default)
    {
        if (!TryResolveDay(date, out var day))
        {
            return null;
        }

        // Hafta dushanbadan boshlanadi: DayOfWeek'da yakshanba = 0, shuning uchun (n + 6) % 7.
        var monday = day.AddDays(-(((int)day.DayOfWeek + 6) % 7));
        var days = Enumerable.Range(0, 7)
            .Select(i => monday.AddDays(i).ToString(DateFormat, CultureInfo.InvariantCulture))
            .ToList();

        var counts = await _uow.Bookings.Query()
            .Where(b => days.Contains(b.BookingDate) && b.Status != BookingConstants.StatusCancelled)
            .GroupBy(b => b.BookingDate)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count, ct);

        // Bron bo'lmagan kun ham 0 bilan qaytadi - grafikda yetti ustun doim to'liq turadi.
        return days
            .Select(d => new WeeklyStatDto { Date = d, Count = counts.GetValueOrDefault(d) })
            .ToList();
    }

    private bool TryResolveDay(string? date, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            day = DateOnly.FromDateTime(DateTime.Now);
            return true;
        }

        if (DateOnly.TryParseExact(date.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
        {
            return true;
        }

        AddError("Sana 'yyyy-MM-dd' formatida bo'lishi kerak.");
        return false;
    }

    public async Task<DashboardSummaryDto?> GetSummaryAsync(string? date, CancellationToken ct = default)
    {
        if (!TryResolveDay(date, out var day))
        {
            return null;
        }

        // BOOKING_DATE matn ko'rinishida ("yyyy-MM-dd") saqlanadi.
        var dayText = day.ToString(DateFormat, CultureInfo.InvariantCulture);
        var dayBookings = _uow.Bookings.Query().Where(b => b.BookingDate == dayText);

        return new DashboardSummaryDto
        {
            TodayBookingsCount = await dayBookings.CountAsync(ct),
            PatientsCount = await _uow.Patients.Query().CountAsync(ct),
            ActiveNursesCount = await _uow.Nurses.Query().CountAsync(n => n.IsActive, ct),
            TotalNursesCount = await _uow.Nurses.Query().CountAsync(ct),
            // MedicineDto.IsLowStock bilan bir xil qoida.
            LowStockCount = await _uow.Medicines.Query().CountAsync(m => m.StockQty <= m.MinStockLevel, ct),
            // Bookings/Revenue bilan bir xil: bekor qilingan bron tushum emas.
            TodayRevenue = await dayBookings
                .Where(b => b.Status != BookingConstants.StatusCancelled)
                .SumAsync(b => b.TotalPrice, ct)
        };
    }
}
