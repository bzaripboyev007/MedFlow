namespace MedFlow.Application.MedFlowServis.DashboardServices;

/// <summary>Haftalik grafikning bitta kuni: sana ("yyyy-MM-dd") va shu kundagi bronlar soni.</summary>
public class WeeklyStatDto
{
    public string Date { get; set; } = null!;
    public int Count { get; set; }
}
