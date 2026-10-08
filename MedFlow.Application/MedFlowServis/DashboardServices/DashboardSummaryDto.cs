namespace MedFlow.Application.MedFlowServis.DashboardServices;

/// <summary>Admin paneli kartalaridagi sonlar - to'liq ro'yxatlar o'rniga bitta yengil so'rov.</summary>
public class DashboardSummaryDto
{
    public int TodayBookingsCount { get; set; }
    public int PatientsCount { get; set; }
    public int ActiveNursesCount { get; set; }
    public int TotalNursesCount { get; set; }
    public int LowStockCount { get; set; }
    public decimal TodayRevenue { get; set; }
}
