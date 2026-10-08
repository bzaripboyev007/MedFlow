using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.NurseServices;

public enum NurseSortField
{
    FullName = 1,
    ExperienceStartDate = 2,
    Rating = 3,
    PricePerVisit = 4,
    HiredAt = 5
}

/// <summary>Hamshiralar ro'yxatidagi qidiruv, filtr, saralash va sahifalash parametrlari.</summary>
public class NurseSortFilterRequest
{
    /// <summary>F.I.O. bo'yicha qidiruv (katta-kichik harf farqsiz, matnning istalgan qismi).</summary>
    public string? Search { get; set; }

    /// <summary>"Pediatric" | "Adult" | null (hammasi).</summary>
    public NurseType? Type { get; set; }

    /// <summary>"Male" | "Female" | null (hammasi).</summary>
    public Gender? Gender { get; set; }

    /// <summary>true - faol, false - ishdan bo'shagan, null - hammasi.</summary>
    public bool? IsActive { get; set; }

    public NurseSortField SortBy { get; set; } = NurseSortField.FullName;

    /// <summary>true - kamayish tartibida.</summary>
    public bool SortDesc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
