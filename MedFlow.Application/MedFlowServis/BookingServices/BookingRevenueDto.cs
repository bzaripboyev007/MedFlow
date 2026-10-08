namespace MedFlow.Application.MedFlowServis.BookingServices;

/// <summary>Bir kunlik tushum: xizmat haqi, uyga chaqirish (yo'l) haqi va bronlar bo'yicha taqsimot.</summary>
public class BookingRevenueDto
{
    public decimal ServiceTotal { get; set; }
    public decimal HomeVisitTotal { get; set; }
    public decimal Total { get; set; }
    public List<BookingRevenueItemDto> Items { get; set; } = new();
}

public class BookingRevenueItemDto
{
    public long BookingId { get; set; }
    public string FullName { get; set; } = null!;
    public string ServiceType { get; set; } = null!;
    public decimal TotalPrice { get; set; }
}
