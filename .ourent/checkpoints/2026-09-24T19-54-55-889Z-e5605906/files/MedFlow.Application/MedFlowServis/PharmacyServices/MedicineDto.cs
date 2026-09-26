namespace MedFlow.Application.MedFlowServis.PharmacyServices;

public class MedicineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public int MinStockLevel { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsExpiringSoon { get; set; }
}
