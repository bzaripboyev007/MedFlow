using MedFlow.Application.MedFlowServis.SessionServices;
using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class CourseDto
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public long PrescriptionId { get; set; }
    public int PrimaryNurseId { get; set; }
    public string PrimaryNurseName { get; set; } = null!;
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = null!;
    public decimal PricePerSession { get; set; }
    public LocationType LocationType { get; set; }
    public string? Address { get; set; }
    public int DurationDays { get; set; }
    public int TimesPerDay { get; set; }
    public DateTime StartDate { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public string ProgressText { get; set; } = null!;
    public CourseStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<SessionDto> Sessions { get; set; } = null!;
}
