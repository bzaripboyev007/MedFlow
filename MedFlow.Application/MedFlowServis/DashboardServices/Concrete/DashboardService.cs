using System.Globalization;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.DashboardServices.Concrete;

public class DashboardService : StatusGenericHandler, IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DashboardSummaryDto?> GetSummaryAsync(string? date, CancellationToken ct = default)
    {
        DateOnly day;
        if (string.IsNullOrWhiteSpace(date))
        {
            day = DateOnly.FromDateTime(DateTime.Now);
        }
        else if (!DateOnly.TryParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
        {
            AddError("Sana 'yyyy-MM-dd' formatida bo'lishi kerak.");
            return null;
        }

        // BOOKING_DATE matn ko'rinishida ("yyyy-MM-dd") saqlanadi.
        var dayText = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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
