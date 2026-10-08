namespace MedFlow.Application.MedFlowServis.NurseServices;

/// <summary>Admin hamshira parolini yangisiga almashtiradi (eski parol so'ralmaydi).</summary>
public class ChangeNursePasswordRequest
{
    public string NewPassword { get; set; } = null!;
}
