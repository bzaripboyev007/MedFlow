using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.NurseServices;
using MedFlow.Application.MedFlowServis.NurseServices.Concrete;
using MedFlow.Auth;
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

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionCode.NurseEdit)]
    public async Task<ActionResult<NurseDto>> Update(int id, [FromBody] UpdateNurseRequest request, CancellationToken ct)
    {
        var result = await _nurses.UpdateAsync(id, request, ct);
        return FromStatus(result, _nurses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<List<BusySlotDto>>> GetAvailability(int id, [FromQuery] DateTime date, CancellationToken ct)
    {
        var result = await _nurses.GetAvailabilityAsync(id, date, ct);
        return FromStatus(result, _nurses);
    }
}
