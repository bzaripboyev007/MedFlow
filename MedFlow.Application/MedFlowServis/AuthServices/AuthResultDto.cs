using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.AuthServices;

public class AuthResultDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public UserRole Role { get; set; }
    public string Panel { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public long? LinkedPatientId { get; set; }
    public int? LinkedNurseId { get; set; }
}
