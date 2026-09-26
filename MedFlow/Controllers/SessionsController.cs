using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Application.MedFlowServis.SessionServices;
using MedFlow.Application.MedFlowServis.SessionServices.Concrete;
using MedFlow.Auth;
using MedFlow.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SessionsController : ApiControllerBase
{
    private readonly ISessionService _sessions;
    private readonly IPatientService _patients;

    public SessionsController(ISessionService sessions, IPatientService patients)
    {
        _sessions = sessions;
        _patients = patients;
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SessionDto>> GetById(long id, CancellationToken ct)
    {
        var (session, error) = await ResolveAccessibleSessionAsync(id, ct);
        if (error is not null)
        {
            return error;
        }

        return Ok(session);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionArrive)]
    public async Task<ActionResult<SessionDto>> Arrive(long id, CancellationToken ct)
    {
        // Permission yolg'iz yetarli emas: hamshira faqat o'ziga biriktirilgan seansni o'zgartiradi (X-6).
        var (_, error) = await ResolveAccessibleSessionAsync(id, ct);
        if (error is not null)
        {
            return error;
        }

        var result = await _sessions.ArriveAsync(id, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionComplete)]
    public async Task<ActionResult<SessionDto>> Complete(long id, [FromBody] CompleteSessionRequest request, CancellationToken ct)
    {
        var (_, error) = await ResolveAccessibleSessionAsync(id, ct);
        if (error is not null)
        {
            return error;
        }

        var result = await _sessions.CompleteAsync(id, request.Notes, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionMarkMissed)]
    public async Task<ActionResult<SessionDto>> MarkMissed(long id, CancellationToken ct)
    {
        var (_, error) = await ResolveAccessibleSessionAsync(id, ct);
        if (error is not null)
        {
            return error;
        }

        var result = await _sessions.MarkMissedAsync(id, ct);
        return FromStatus(result, _sessions);
    }

    [HttpPost("{id:long}")]
    [Authorize(Policy = PermissionCode.SessionIssueMedicine)]
    public async Task<ActionResult<List<MedicineIssueDto>>> IssueMedicine(long id, [FromBody] IssueMedicineRequest request, CancellationToken ct)
    {
        var (_, error) = await ResolveAccessibleSessionAsync(id, ct);
        if (error is not null)
        {
            return error;
        }

        var result = await _sessions.IssueMedicineAsync(id, request.MedicineId, request.Quantity, ct);
        return FromStatus(result, _sessions);
    }

    /// <summary>
    /// Seansni yuklaydi va egalikni tekshiradi (IDOR himoyasi). Muvaffaqiyatda (session, null);
    /// topilmasa/xato bo'lsa (null, ValidationProblem); egalik yo'q bo'lsa (null, Forbid).
    /// Kurs egaligi bilan bir xil qoida (CoursesController.CanAccessCourseAsync):
    /// Admin - hammasi, hamshira - faqat o'z seansi, bemor - faqat o'zi/farzandi.
    /// </summary>
    private async Task<(SessionDto? Session, ActionResult? Error)> ResolveAccessibleSessionAsync(long id, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(id, ct);
        if (session is null || !_sessions.IsValid)
        {
            _sessions.CopyErrorsToModelState(ModelState);
            return (null, ValidationProblem(ModelState));
        }

        var allowed = User.IsInRole("Admin")
                      || (User.IsNurse() && User.GetLinkedNurseId() == session.NurseId)
                      || (User.IsPatient() && User.GetLinkedPatientId() is long patientId
                          && await _patients.IsSelfOrChildAsync(patientId, session.PatientId, ct));

        return allowed ? (session, null) : (null, Forbid());
    }
}
