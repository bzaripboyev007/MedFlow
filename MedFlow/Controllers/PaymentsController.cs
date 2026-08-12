using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Application.MedFlowServis.PaymentServices;
using MedFlow.Application.MedFlowServis.PaymentServices.Concrete;
using MedFlow.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _payments;
    private readonly IPatientService _patients;

    public PaymentsController(IPaymentService payments, IPatientService patients)
    {
        _payments = payments;
        _patients = patients;
    }

    [HttpPost("{courseId:long}")]
    [Authorize(Policy = PermissionCode.PaymentCreate)]
    public async Task<ActionResult<PaymentDto>> CreateForCourse(long courseId, [FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var result = await _payments.CreateForCourseAsync(courseId, request.Method, ct);
        if (!_payments.IsValid)
        {
            return FromStatus(result, _payments);
        }

        return CreatedAtAction(nameof(GetByCourse), new { courseId = result!.CourseId }, result);
    }

    [HttpGet("{courseId:long}")]
    public async Task<ActionResult<PaymentDto>> GetByCourse(long courseId, CancellationToken ct)
    {
        var payment = await _payments.GetByCourseAsync(courseId, ct);
        if (!_payments.IsValid)
        {
            return FromStatus(payment, _payments);
        }

        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, payment!.PatientId, ct))
        {
            return Forbid();
        }

        return Ok(payment);
    }

    [HttpGet("{patientId:long}")]
    public async Task<ActionResult<List<PaymentDto>>> ListByPatient(long patientId, CancellationToken ct)
    {
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, patientId, ct))
        {
            return Forbid();
        }

        var result = await _payments.ListByPatientAsync(patientId, ct);
        return FromStatus(result, _payments);
    }
}
