using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class CreateCourseRequest
{
    public long PatientId { get; set; }
    public long PrescriptionId { get; set; }
    public int ServiceTypeId { get; set; }
    public LocationType LocationType { get; set; }
    public string? Address { get; set; }
    public int DurationDays { get; set; }
    public int TimesPerDay { get; set; }

    /// <summary>"HH:mm" formatida, TimesPerDay bilan bir xil uzunlikda. Yuborilmasa CourseService 400 qaytaradi.</summary>
    public List<string>? PreferredTimes { get; set; }

    public DateTime StartDate { get; set; }

    /// <summary>Faqat kattalar bemori uchun majburiy.</summary>
    public Gender? DesiredNurseGender { get; set; }

    public AssignmentMode AssignmentMode { get; set; }

    /// <summary>AssignmentMode=Manual bo'lsa majburiy.</summary>
    public int? RequestedNurseId { get; set; }
}
