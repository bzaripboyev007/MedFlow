namespace MedFlow.Application.MedFlowServis.AuthServices;

public class LoginRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}
