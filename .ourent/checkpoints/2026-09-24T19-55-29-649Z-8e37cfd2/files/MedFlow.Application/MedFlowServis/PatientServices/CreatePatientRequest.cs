namespace MedFlow.Application.MedFlowServis.PatientServices;

public class CreatePatientRequest
{
    public string FullName { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; } = null!;
    public long? ParentId { get; set; }
    public bool IsChild { get; set; }
    public string Allergies { get; set; } = null!;
}
