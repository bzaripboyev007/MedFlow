namespace MedFlow.Application.MedFlowServis.PatientServices;

public class UpdatePatientRequest
{
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Allergies { get; set; } = null!;
}
