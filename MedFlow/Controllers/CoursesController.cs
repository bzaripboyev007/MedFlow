using MedFlow.Application.Common.Permissions;
using MedFlow.Application.MedFlowServis.CourseServices;
using MedFlow.Application.MedFlowServis.CourseServices.Concrete;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Application.MedFlowServis.SessionServices;
using MedFlow.Application.MedFlowServis.SessionServices.Concrete;
using MedFlow.Auth;
using MedFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedFlow.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _courses;
    private readonly ISessionService _sessions;
    private readonly IPatientService _patients;

    public CoursesController(ICourseService courses, ISessionService sessions, IPatientService patients)
    {
        _courses = courses;
        _sessions = sessions;
        _patients = patients;
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        if (User.IsPatient() && !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, request.PatientId, ct))
        {
            return Forbid();
        }

        var result = await _courses.CreateAsync(request, ct);
        if (!_courses.IsValid)
        {
            return FromStatus(result, _courses);
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CourseDto>> GetById(long id, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (!_courses.IsValid)
        {
            return FromStatus(course, _courses);
        }

        if (!await CanAccessCourseAsync(course!.PatientId, course.PrimaryNurseId, ct))
        {
            return Forbid();
        }

        return Ok(course);
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseSummaryDto>>> List(
        [FromQuery] long? patientId, [FromQuery] int? nurseId, [FromQuery] CourseStatus? status, CancellationToken ct)
    {
        if (User.IsPatient())
        {
            if (patientId is null || !await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, patientId.Value, ct))
            {
                return Forbid();
            }
        }
        else if (User.IsNurse())
        {
            // Hamshira faqat o'ziga biriktirilgan kurslarni ko'radi (spec 5a).
            nurseId = User.GetLinkedNurseId();
        }

        var result = await _courses.ListAsync(patientId, nurseId, status, ct);
        return FromStatus(result, _courses);
    }

    [HttpPatch("{id:long}")]
    [Authorize(Policy = PermissionCode.CourseEdit)]
    public async Task<ActionResult<CourseDto>> UpdateStatus(long id, [FromBody] UpdateCourseStatusRequest request, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (!_courses.IsValid)
        {
            return FromStatus(course, _courses);
        }

        // Permission yetarli emas: hamshira faqat o'ziga biriktirilgan kurs holatini o'zgartiradi.
        if (!await CanAccessCourseAsync(course!.PatientId, course.PrimaryNurseId, ct))
        {
            return Forbid();
        }

        var result = await _courses.UpdateStatusAsync(id, request.Status, ct);
        return FromStatus(result, _courses);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CourseWalletSummaryDto>> GetWalletSummary(long id, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (!_courses.IsValid)
        {
            return FromStatus<CourseWalletSummaryDto?>(null, _courses);
        }

        if (!await CanAccessCourseAsync(course!.PatientId, course.PrimaryNurseId, ct))
        {
            return Forbid();
        }

        var result = await _courses.GetWalletSummaryAsync(id, ct);
        return FromStatus(result, _courses);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<List<SessionDto>>> GetSessions(long id, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(id, ct);
        if (!_courses.IsValid)
        {
            return FromStatus<List<SessionDto>?>(null, _courses);
        }

        if (!await CanAccessCourseAsync(course!.PatientId, course.PrimaryNurseId, ct))
        {
            return Forbid();
        }

        var result = await _sessions.ListByCourseAsync(id, ct);
        return FromStatus(result, _sessions);
    }

    private async Task<bool> CanAccessCourseAsync(long coursePatientId, int coursePrimaryNurseId, CancellationToken ct)
    {
        if (User.IsInRole("Admin")) return true;
        if (User.IsNurse()) return User.GetLinkedNurseId() == coursePrimaryNurseId;
        if (User.IsPatient()) return await _patients.IsSelfOrChildAsync(User.GetLinkedPatientId()!.Value, coursePatientId, ct);
        return false;
    }
}
