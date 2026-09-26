using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.SessionServices.Concrete;

public interface ISessionService : IStatusGeneric
{
    Task<SessionDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<SessionDto>> ListByCourseAsync(long courseId, CancellationToken ct = default);
    Task<SessionDto?> ArriveAsync(long sessionId, CancellationToken ct = default);
    Task<SessionDto?> CompleteAsync(long sessionId, string? notes, CancellationToken ct = default);
    Task<SessionDto?> MarkMissedAsync(long sessionId, CancellationToken ct = default);
    Task<List<MedicineIssueDto>> IssueMedicineAsync(long sessionId, int medicineId, int quantity, CancellationToken ct = default);
    Task<int> SweepMissedSessionsAsync(CancellationToken ct = default);
}
