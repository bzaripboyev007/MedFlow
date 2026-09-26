using MedFlow.Application.MedFlowServis.BookingServices;
using MedFlow.Application.MedFlowServis.BookingServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class BookingsController : ApiControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings)
    {
        _bookings = bookings;
    }

    /// <summary>Onlayn bron so'rovi - ro'yxatdan o'tmagan mehmon ham yubora oladi.</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await _bookings.CreateAsync(request, ct);
        if (!_bookings.IsValid)
        {
            return FromStatus(result, _bookings);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    /// <summary>Bronlar ro'yxati. Hamshira faqat o'ziga biriktirilganlarini ko'radi.</summary>
    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> List([FromQuery] int? nurseId, CancellationToken ct)
    {
        var effectiveNurseId = User.IsNurse() ? User.GetLinkedNurseId() : nurseId;
        var result = await _bookings.ListAsync(effectiveNurseId, ct);
        return FromStatus(result, _bookings);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<BookingDto>> GetById(long id, CancellationToken ct)
    {
        var result = await _bookings.GetByIdAsync(id, ct);
        return FromStatus(result, _bookings);
    }

    /// <summary>Hamshira bron holatini o'zgartiradi (Yangi -> Kelgan -> Jarayonda -> Yakunlandi).</summary>
    [HttpPatch("{id:long}")]
    public async Task<ActionResult<BookingDto>> UpdateStatus(long id, [FromBody] UpdateBookingStatusRequest request, CancellationToken ct)
    {
        var result = await _bookings.UpdateStatusAsync(id, request.Status, ct);
        return FromStatus(result, _bookings);
    }
}
