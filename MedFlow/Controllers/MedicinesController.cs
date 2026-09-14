using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.PharmacyServices;
using MedFlow.Application.MedFlowServis.PharmacyServices.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Policy = PermissionCode.MedicineManage)]
public class MedicinesController : ApiControllerBase
{
    private readonly IPharmacyService _pharmacy;

    public MedicinesController(IPharmacyService pharmacy)
    {
        _pharmacy = pharmacy;
    }

    [HttpPost]
    public async Task<ActionResult<MedicineDto>> Create([FromBody] CreateMedicineRequest request, CancellationToken ct)
    {
        var result = await _pharmacy.CreateAsync(request, ct);
        if (!_pharmacy.IsValid)
        {
            return FromStatus(result, _pharmacy);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicineDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _pharmacy.GetByIdAsync(id, ct);
        return FromStatus(result, _pharmacy);
    }

    [HttpGet]
    public async Task<ActionResult<List<MedicineDto>>> List(CancellationToken ct)
    {
        var result = await _pharmacy.ListAsync(ct);
        return FromStatus(result, _pharmacy);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MedicineDto>> Update(int id, [FromBody] UpdateMedicineRequest request, CancellationToken ct)
    {
        var result = await _pharmacy.UpdateAsync(id, request, ct);
        return FromStatus(result, _pharmacy);
    }

    [HttpPost("{id:int}")]
    public async Task<ActionResult<MedicineDto>> StockIn(int id, [FromBody] StockInRequest request, CancellationToken ct)
    {
        var result = await _pharmacy.StockInAsync(id, request.Quantity, ct);
        return FromStatus(result, _pharmacy);
    }

    [HttpGet]
    public async Task<ActionResult<List<MedicineDto>>> GetAlerts(CancellationToken ct)
    {
        var result = await _pharmacy.GetAlertsAsync(ct);
        return FromStatus(result, _pharmacy);
    }
}
