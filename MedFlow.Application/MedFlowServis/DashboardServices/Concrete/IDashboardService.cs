using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.DashboardServices.Concrete;

public interface IDashboardService : IStatusGeneric
{
    /// <summary>Kartalar uchun sonlar. Sana ("yyyy-MM-dd") berilmasa - bugun.</summary>
    Task<DashboardSummaryDto?> GetSummaryAsync(string? date, CancellationToken ct = default);
}
