using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.SessionServices;

public class SessionDto
{
    public long Id { get; set; }
    public long CourseId { get; set; }
    public int SeqNo { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? ActualEndAt { get; set; }
    public int NurseId { get; set; }
    public string NurseName { get; set; } = null!;
    public SessionStatus Status { get; set; }
    public string? Notes { get; set; }
}
