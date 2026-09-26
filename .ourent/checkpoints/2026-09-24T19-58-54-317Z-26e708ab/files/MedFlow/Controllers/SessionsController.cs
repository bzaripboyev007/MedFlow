using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.SessionServices;
using MedFlow.Application.MedFlowServis.SessionServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SessionsController : ApiControllerBase
{
    private readonly ISessionService _sessions;

    public SessionsController(ISessionService sessions)
    {
        _sessions = sessions;
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SessionDto>> GetById(long id, CancellationToken ct)
    {
        var result = await _sessions.GetByIdAsync(id, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionArrive)]
    public async Task<ActionResult<SessionDto>> Arrive(long id, CancellationToken ct)
    {
        var result = await _sessions.ArriveAsync(id, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionComplete)]
    public async Task<ActionResult<SessionDto>> Complete(long id, [FromBody] CompleteSessionRequest request, CancellationToken ct)
    {
        var result = await _sessions.CompleteAsync(id, request.Notes, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionMarkMissed)]
    public async Task<ActionResult<SessionDto>> MarkMissed(long id, CancellationToken ct)
    {
        var result = await _sessions.MarkMissedAsync(id, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionIssueMedicine)]
    public async Task<ActionResult<List<MedicineIssueDto>>> IssueMedicine(long id, [FromBody] IssueMedicineRequest request, CancellationToken ct)
    {
        var result = await _sessions.IssueMedicineAsync(id, request.MedicineId, request.Quantity, ct);
        return FromStatus(result, _sessions);
    }
}
