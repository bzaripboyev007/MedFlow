namespace MedFlow.Application.MedFlowServis.ServiceTypeServices;

public class ServiceTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal PricePerSession { get; set; }
    public bool IsActive { get; set; }
}
