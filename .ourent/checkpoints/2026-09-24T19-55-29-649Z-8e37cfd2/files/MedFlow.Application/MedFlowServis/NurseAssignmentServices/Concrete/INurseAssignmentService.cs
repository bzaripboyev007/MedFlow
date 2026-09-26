using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.NurseAssignmentServices.Concrete;

public interface INurseAssignmentService : IStatusGeneric
{
    /// <summary>
    /// Kurs uchun asosiy hamshirani biriktiradi. AssignmentMode=Auto bo'lsa - bo'sh + mos + oldingi
    /// tanishlik ustunligi bilan avtomatik tanlaydi; Manual bo'lsa - berilgan hamshirani validatsiya qiladi.
    /// </summary>
    Task<Nurse?> AssignAsync(
        Patient patient,
        Gender? desiredGender,
        IReadOnlyList<DateTimeOffset> sessionTimes,
        AssignmentMode mode,
        int? requestedNurseId,
        CancellationToken ct = default);
}
