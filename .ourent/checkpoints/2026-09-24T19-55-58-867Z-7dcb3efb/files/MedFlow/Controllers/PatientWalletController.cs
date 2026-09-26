using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Application.MedFlowServis.WalletServices;
using MedFlow.Application.MedFlowServis.WalletServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PatientWalletController : ApiControllerBase
{
    private readonly IWalletService _wallet;
    private readonly IPatientService _patients;

    public PatientWalletController(IWalletService wallet, IPatientService patients)
    {
        _wallet = wallet;
        _patients = patients;
    }

    [HttpPost("{patientId:long}")]
    [Authorize(Policy = PermissionCode.WalletPurchase)]
    public async Task<ActionResult<WalletEntryDto>> Purchase(long patientId, [FromBody] PurchaseWalletRequest request, CancellationToken ct)
    {
        var result = await _wallet.PurchaseAsync(patientId, request, ct);
        return FromStatus(result, _wallet);
    }

    [HttpGet("{patientId:long}")]
    public async Task<ActionResult<List<WalletEntryDto>>> Get(long patientId, CancellationToken ct)
    {
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, patientId, ct))
        {
            return Forbid();
        }

        var result = await _wallet.GetWalletAsync(patientId, ct);
        return FromStatus(result, _wallet);
    }
}
