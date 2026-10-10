namespace MedFlow.Application.MedFlowServis.PatientServices;

public class PatientDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string Phone { get; set; } = null!;
    public long? ParentId { get; set; }
    public bool IsChild { get; set; }
    public string Allergies { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Oxirgi tashrif sanasi ("yyyy-MM-dd"): yakunlangan kurs seansi yoki bemor kelgan bron
    /// (telefon raqami bo'yicha) - qaysi biri keyinroq bo'lsa. Hech qachon kelmagan bo'lsa null.
    /// </summary>
    public DateTime? LastVisitDate { get; set; }

    /// <summary>
    /// Muolaja boshlangan sana va vaqt ("yyyy-MM-ddTHH:mm:ss", klinika mahalliy vaqti): bemorning
    /// eng birinchi tashrifi - birinchi boshlangan kurs seansi yoki u kelgan birinchi bron.
    /// Hali muolaja boshlamagan bo'lsa null.
    /// </summary>
    public string? TreatmentStartedAt { get; set; }
}
