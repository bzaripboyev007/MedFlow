namespace MedFlow.Application.MedFlowServis.AuthServices;

public class RegisterPatientRequest
{
    public string FullName { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
    public long? ParentId { get; set; }
    public bool IsChild { get; set; }
    public string? Allergies { get; set; }
}
