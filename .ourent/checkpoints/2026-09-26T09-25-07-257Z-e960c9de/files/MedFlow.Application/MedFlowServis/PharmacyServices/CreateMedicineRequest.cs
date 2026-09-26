namespace MedFlow.Application.MedFlowServis.PharmacyServices;

public class CreateMedicineRequest
{
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public int MinStockLevel { get; set; }
    public DateTime ExpiryDate { get; set; }
}
