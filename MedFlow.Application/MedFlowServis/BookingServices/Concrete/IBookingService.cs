using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.BookingServices.Concrete;

public interface IBookingService : IStatusGeneric
{
    Task<BookingDto?> CreateAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<List<BookingDto>> ListAsync(int? nurseId = null, CancellationToken ct = default);
    Task<BookingDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<BookingDto?> UpdateStatusAsync(long id, string status, CancellationToken ct = default);

    /// <summary>Berilgan kun ("yyyy-MM-dd") tushumi. Bekor qilingan bronlar hisobga kirmaydi. Sana berilmasa - bugun.</summary>
    Task<BookingRevenueDto?> GetRevenueAsync(string? date, CancellationToken ct = default);
}
