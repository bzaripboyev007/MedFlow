using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class CourseSummaryDto
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public int PrimaryNurseId { get; set; }
    public string PrimaryNurseName { get; set; } = null!;
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public CourseStatus Status { get; set; }
    public DateTime StartDate { get; set; }
}
