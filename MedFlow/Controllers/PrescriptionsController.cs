using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Application.MedFlowServis.PrescriptionServices;
using MedFlow.Application.MedFlowServis.PrescriptionServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PrescriptionsController : ApiControllerBase
{
    private readonly IPrescriptionService _prescriptions;
    private readonly IPatientService _patients;

    public PrescriptionsController(IPrescriptionService prescriptions, IPatientService patients)
    {
        _prescriptions = prescriptions;
        _patients = patients;
    }

    [HttpPost("{patientId:long}")]
    public async Task<ActionResult<PrescriptionDto>> Upload(long patientId, IFormFile file, CancellationToken ct)
    {
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, patientId, ct))
        {
            return Forbid();
        }

        if (file.Length == 0)
        {
            return BadRequest(new { error = "Fayl bo'sh." });
        }

        await using var stream = file.OpenReadStream();
        var result = await _prescriptions.UploadAsync(patientId, stream, file.FileName, file.ContentType, ct);
        if (!_prescriptions.IsValid)
        {
            return FromStatus(result, _prescriptions);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PrescriptionDto>> GetById(long id, CancellationToken ct)
    {
        var result = await _prescriptions.GetByIdAsync(id, ct);
        if (!_prescriptions.IsValid)
        {
            return FromStatus(result, _prescriptions);
        }

        // IDOR himoyasi: bemor faqat o'zining yoki farzandining retseptini ochadi
        // (Upload/ListByPatient bilan bir xil qoida).
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, result!.PatientId, ct))
        {
            return Forbid();
        }

        return Ok(result);
    }

    [HttpGet("{patientId:long}")]
    public async Task<ActionResult<List<PrescriptionDto>>> ListByPatient(long patientId, CancellationToken ct)
    {
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, patientId, ct))
        {
            return Forbid();
        }

        var result = await _prescriptions.ListByPatientAsync(patientId, ct);
        return FromStatus(result, _prescriptions);
    }

    [HttpPatch("{id:long}")]
    [Authorize(Policy = PermissionCode.PrescriptionVerify)]
    public async Task<ActionResult<PrescriptionDto>> Verify(long id, [FromBody] VerifyPrescriptionRequest request, CancellationToken ct)
    {
        // Hamshira faqat o'z nomidan tasdiqlaydi; boshqa hamshira Id sini faqat Admin ko'rsata oladi.
        var nurseId = User.IsNurse() ? User.GetLinkedNurseId() ?? request.NurseId : request.NurseId;
        var result = await _prescriptions.VerifyAsync(id, nurseId, ct);
        return FromStatus(result, _prescriptions);
    }
}
