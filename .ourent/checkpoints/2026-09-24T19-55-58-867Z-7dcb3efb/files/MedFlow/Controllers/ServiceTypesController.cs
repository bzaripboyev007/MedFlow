using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.ServiceTypeServices;
using MedFlow.Application.MedFlowServis.ServiceTypeServices.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ServiceTypesController : ApiControllerBase
{
    private readonly IServiceTypeService _serviceTypes;

    public ServiceTypesController(IServiceTypeService serviceTypes)
    {
        _serviceTypes = serviceTypes;
    }

    [HttpPost]
    [Authorize(Policy = PermissionCode.ServiceTypeCreate)]
    public async Task<ActionResult<ServiceTypeDto>> Create([FromBody] CreateServiceTypeRequest request, CancellationToken ct)
    {
        var result = await _serviceTypes.CreateAsync(request, ct);
        if (!_serviceTypes.IsValid)
        {
            return FromStatus(result, _serviceTypes);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceTypeDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _serviceTypes.GetByIdAsync(id, ct);
        return FromStatus(result, _serviceTypes);
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceTypeDto>>> List(CancellationToken ct)
    {
        var result = await _serviceTypes.ListAsync(ct);
        return FromStatus(result, _serviceTypes);
    }
}
