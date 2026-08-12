using MedFlow.Application.MedFlowServis.AuthServices;
using MedFlow.Application.MedFlowServis.AuthServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResultDto>> RegisterPatient([FromBody] RegisterPatientRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterPatientAsync(request, ct);
        return FromStatus(result, _auth);
    }

    [HttpPost]
    public async Task<ActionResult<AuthResultDto>> RegisterAdmin([FromBody] RegisterAdminRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAdminAsync(request, User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"), ct);
        return FromStatus(result, _auth);
    }

    [HttpPost]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return FromStatus(result, _auth);
    }

    [HttpPost]
    public async Task<ActionResult<AuthResultDto>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await _auth.RefreshAsync(request, ct);
        return FromStatus(result, _auth);
    }
}
