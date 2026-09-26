namespace MedFlow.Application.MedFlowServis.AuthServices;

public class RegisterAdminRequest
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}
