namespace MedFlow.Application.MedFlowServis.BookingServices;

public class CreateBookingRequest
{
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;

    /// <summary>"adult" yoki "child".</summary>
    public string PatientType { get; set; } = null!;

    /// <summary>"home" yoki "clinic".</summary>
    public string ServiceType { get; set; } = null!;

    public string? Address { get; set; }
    public string? Landmark { get; set; }
    public int NurseId { get; set; }

    /// <summary>"yyyy-MM-dd".</summary>
    public string BookingDate { get; set; } = null!;

    /// <summary>"HH:mm".</summary>
    public string BookingTime { get; set; } = null!;

    public string? PrescriptionFile { get; set; }
    public string? PrescriptionUrl { get; set; }
}
