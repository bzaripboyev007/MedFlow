using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.AuthServices.Concrete;

public interface IAuthService : IStatusGeneric
{
    Task<AuthResultDto?> RegisterPatientAsync(RegisterPatientRequest request, CancellationToken ct = default);
    Task<AuthResultDto?> RegisterAdminAsync(RegisterAdminRequest request, bool callerIsAdmin, CancellationToken ct = default);
    Task<AuthResultDto?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResultDto?> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
}
