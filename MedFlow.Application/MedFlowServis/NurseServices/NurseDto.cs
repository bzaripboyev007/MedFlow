using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.NurseServices;

public class NurseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public NurseType Type { get; set; }
    public Gender? Gender { get; set; }
    public string Phone { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public DateTime ExperienceStartDate { get; set; }
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public int CompletedCoursesCount { get; set; }
    public bool IsActive { get; set; }
}
