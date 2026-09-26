namespace MedFlow.Application.MedFlowServis.AuthServices;

public class ResetPasswordRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
