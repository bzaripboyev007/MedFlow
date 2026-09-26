using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.NurseServices;

public class CreateNurseRequest
{
    public string FullName { get; set; } = null!;
    public NurseType Type { get; set; }
    public Gender? Gender { get; set; }
    public string Phone { get; set; } = null!;
    public string Specialization { get; set; } = null!;
    public DateTime ExperienceStartDate { get; set; }
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;

    /// <summary>Bitta tashrif narxi. 0 bo'lsa standart narx qo'yiladi.</summary>
    public decimal PricePerVisit { get; set; }
}
