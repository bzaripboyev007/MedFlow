namespace MedFlow.Application.MedFlowServis.NurseServices;

public class UpdateNurseRequest
{
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public bool IsActive { get; set; }
    public decimal Rating { get; set; }
    public decimal PricePerVisit { get; set; }
}
