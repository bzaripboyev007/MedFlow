namespace MedFlow.Application.MedFlowServis.PatientServices;

public enum PatientSortField
{
    FullName = 1,
    BirthDate = 2,
    CreatedAt = 3
}

/// <summary>Bemorlar panelidagi qidiruv, filtr, saralash va sahifalash parametrlari.</summary>
public class PatientSortFilterRequest
{
    /// <summary>F.I.O. bo'yicha qidiruv (katta-kichik harf farqsiz, matnning istalgan qismi).</summary>
    public string? Search { get; set; }

    /// <summary>true - faqat bolalar, false - faqat kattalar, null - hammasi.</summary>
    public bool? IsChild { get; set; }

    public PatientSortField SortBy { get; set; } = PatientSortField.FullName;

    /// <summary>true - kamayish tartibida (Z-A, yangidan eskiga).</summary>
    public bool SortDesc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
