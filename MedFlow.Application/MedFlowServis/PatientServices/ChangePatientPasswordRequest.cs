namespace MedFlow.Application.MedFlowServis.PatientServices;

/// <summary>Admin bemor parolini yangisiga almashtiradi (eski parol so'ralmaydi).</summary>
public class ChangePatientPasswordRequest
{
    public string NewPassword { get; set; } = null!;
}
