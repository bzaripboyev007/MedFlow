using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.NurseServices;

/// <summary>
/// Bron sahifasida (login qilmagan mehmonga) ko'rsatiladigan hamshira ma'lumoti.
/// Xodimning telefon raqami va login'i ataylab kiritilmagan.
/// </summary>
public class PublicNurseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public NurseType Type { get; set; }
    public Gender? Gender { get; set; }
    public string Specialization { get; set; } = null!;
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public decimal PricePerVisit { get; set; }
}
