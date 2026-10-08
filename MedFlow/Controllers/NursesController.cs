using MedFlow.Application.Common;
using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.NurseServices;
using MedFlow.Application.MedFlowServis.NurseServices.Concrete;
using MedFlow.Auth;
using MedFlow.Extensions;
using MedFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class NursesController : ApiControllerBase
{
    private readonly INurseService _nurses;

    public NursesController(INurseService nurses)
    {
        _nurses = nurses;
    }

    [HttpPost]
    [Authorize(Policy = PermissionCode.NurseCreate)]
    public async Task<ActionResult<NurseDto>> Create([FromBody] CreateNurseRequest request, CancellationToken ct)
    {
        var result = await _nurses.CreateAsync(request, ct);
        if (!_nurses.IsValid)
        {
            return FromStatus(result, _nurses);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NurseDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _nurses.GetByIdAsync(id, ct);
        return FromStatus(result, _nurses);
    }

    [HttpGet]
    public async Task<ActionResult<List<NurseDto>>> List([FromQuery] NurseType? type, [FromQuery] Gender? gender, CancellationToken ct)
    {
        var result = await _nurses.ListAsync(type, gender, ct);
        return FromStatus(result, _nurses);
    }

    /// <summary>Bron sahifasi uchun faol hamshiralar (mehmon ham ko'ra oladi, telefon raqamisiz).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<PublicNurseDto>>> PublicList([FromQuery] NurseType? type, [FromQuery] Gender? gender, CancellationToken ct)
    {
        var result = await _nurses.ListPublicAsync(type, gender, ct);
        return FromStatus(result, _nurses);
    }

    /// <summary>Barcha hamshiralar (F.I.O. bo'yicha alifbo tartibida, ishdan bo'shaganlar ham). Faqat Admin.</summary>
    [HttpGet]
    [Authorize(Roles = RoleGroups.Admin)]
    public async Task<ActionResult<List<NurseDto>>> GetList(CancellationToken ct)
    {
        var result = await _nurses.ListAsync(null, null, ct);
        return FromStatus(result, _nurses);
    }

    /// <summary>Hamshiralar ro'yxati: F.I.O. qidiruvi, toifa/jins/holat filtri, saralash va sahifalash. Faqat Admin.</summary>
    [HttpPost]
    [Authorize(Roles = RoleGroups.Admin)]
    public async Task<ActionResult<PagedResultDto<NurseDto>>> SortFilter([FromBody] NurseSortFilterRequest request, CancellationToken ct)
    {
        var result = await _nurses.SortFilterAsync(request, ct);
        return FromStatus(result, _nurses);
    }

    /// <summary>Admin hamshira parolini almashtiradi; hamshiraning eski sessiyalari yopiladi.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleGroups.Admin)]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangeNursePasswordRequest request, CancellationToken ct)
    {
        await _nurses.ChangePasswordAsync(id, request.NewPassword, ct);
        if (!_nurses.IsValid)
        {
            _nurses.CopyErrorsToModelState(ModelState);
            return ValidationProblem(ModelState);
        }

        return NoContent();
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionCode.NurseEdit)]
    public async Task<ActionResult<NurseDto>> Update(int id, [FromBody] UpdateNurseRequest request, CancellationToken ct)
    {
        var result = await _nurses.UpdateAsync(id, request, ct);
        return FromStatus(result, _nurses);
    }

    /// <summary>Hamshiraning bir kunda band vaqtlari (seanslar + faol bronlar). "availability" aniq route,
    /// shuning uchun GetById bilan to'qnashmaydi.</summary>
    [HttpGet("availability/{id:int}")]
    [Authorize(Policy = PermissionCode.BookingView)]
    public async Task<ActionResult<List<BusySlotDto>>> GetAvailability(int id, [FromQuery] DateTime date, CancellationToken ct)
    {
        var result = await _nurses.GetAvailabilityAsync(id, date, ct);
        return FromStatus(result, _nurses);
    }
}
