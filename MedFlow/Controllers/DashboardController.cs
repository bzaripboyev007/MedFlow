using MedFlow.Application.MedFlowServis.DashboardServices;
using MedFlow.Application.MedFlowServis.DashboardServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = RoleGroups.Admin)]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    /// <summary>Admin paneli kartalaridagi sonlar. Sana "yyyy-MM-dd"; berilmasa - bugun.</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> Summary([FromQuery] string? date, CancellationToken ct)
    {
        var result = await _dashboard.GetSummaryAsync(date, ct);
        return FromStatus(result, _dashboard);
    }
}
