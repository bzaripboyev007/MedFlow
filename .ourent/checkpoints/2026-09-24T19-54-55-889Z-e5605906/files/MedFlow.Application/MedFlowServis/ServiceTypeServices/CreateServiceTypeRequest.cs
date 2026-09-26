namespace MedFlow.Application.MedFlowServis.ServiceTypeServices;

public class CreateServiceTypeRequest
{
    public string Name { get; set; } = null!;
    public decimal PricePerSession { get; set; }
}
