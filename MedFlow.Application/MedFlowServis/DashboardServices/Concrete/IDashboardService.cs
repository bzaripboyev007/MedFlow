using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.DashboardServices.Concrete;

public interface IDashboardService : IStatusGeneric
{
    /// <summary>Kartalar uchun sonlar. Sana ("yyyy-MM-dd") berilmasa - bugun.</summary>
    Task<DashboardSummaryDto?> GetSummaryAsync(string? date, CancellationToken ct = default);

    /// <summary>
    /// Haftaning yetti kuni (dushanbadan yakshanbagacha) bo'yicha bronlar soni, bekor qilinganlarsiz.
    /// Sana ("yyyy-MM-dd") berilsa - shu sana tushgan hafta, berilmasa - joriy hafta.
    /// </summary>
    Task<List<WeeklyStatDto>?> GetWeeklyStatsAsync(string? date, CancellationToken ct = default);
}
