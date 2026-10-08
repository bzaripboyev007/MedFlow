using MedFlow.Application.Common;
using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.PatientServices;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Auth;
using MedFlow.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PatientsController : ApiControllerBase
{
    private readonly IPatientService _patients;

    public PatientsController(IPatientService patients)
    {
        _patients = patients;
    }

    [HttpPost]
    [Authorize(Roles = RoleGroups.Staff)]
    public async Task<ActionResult<PatientDto>> CreateAsync([FromBody] CreatePatientRequest request, CancellationToken ct)
    {
        var result = await _patients.CreateAsync(request, ct);
        if (!_patients.IsValid)
        {
            return FromStatus(result, _patients);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PatientDto>> GetById(long id, CancellationToken ct)
    {
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, id, ct))
        {
            return Forbid();
        }

        var result = await _patients.GetByIdAsync(id, ct);
        return FromStatus(result, _patients);
    }

    [HttpGet]
    public async Task<ActionResult<List<PatientDto>>> List([FromQuery] long? parentId, CancellationToken ct)
    {
        // Bemor faqat o'z farzandlari ro'yxatini ko'rishi mumkin (spec 5a: ota-ona faqat o'ziga bog'langan profillarga kiradi).
        // Bemor kartasiga bog'lanmagan bemor hisobi uchun filtr null bo'lib, barcha bemorlar qaytib ketmasin.
        if (User.IsPatient() && User.GetLinkedPatientId() is null)
        {
            return Forbid();
        }

        var effectiveParentId = User.IsPatient() ? User.GetLinkedPatientId() : parentId;
        var result = await _patients.ListAsync(effectiveParentId, ct);
        return FromStatus(result, _patients);
    }

    /// <summary>Barcha bemorlar (F.I.O. bo'yicha alifbo tartibida). Faqat xodimlar uchun.</summary>
    [HttpGet]
    [Authorize(Roles = RoleGroups.Staff)]
    public async Task<ActionResult<List<PatientDto>>> GetList(CancellationToken ct)
    {
        var result = await _patients.ListAsync(null, ct);
        return FromStatus(result, _patients);
    }

    /// <summary>Bemorlar paneli: F.I.O. qidiruvi, toifa filtri, saralash va sahifalash. Faqat xodimlar uchun.</summary>
    [HttpPost]
    [Authorize(Roles = RoleGroups.Staff)]
    public async Task<ActionResult<PagedResultDto<PatientDto>>> SortFilter([FromBody] PatientSortFilterRequest request, CancellationToken ct)
    {
        var result = await _patients.SortFilterAsync(request, ct);
        return FromStatus(result, _patients);
    }

    /// <summary>Admin bemor parolini almashtiradi; bemorning eski sessiyalari yopiladi.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = RoleGroups.Admin)]
    public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePatientPasswordRequest request, CancellationToken ct)
    {
        await _patients.ChangePasswordAsync(id, request.NewPassword, ct);
        if (!_patients.IsValid)
        {
            _patients.CopyErrorsToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        return NoContent();
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PermissionCode.PatientEdit)]
    public async Task<ActionResult<PatientDto>> Update(long id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var result = await _patients.UpdateAsync(id, request, ct);
        return FromStatus(result, _patients);
    }
}
