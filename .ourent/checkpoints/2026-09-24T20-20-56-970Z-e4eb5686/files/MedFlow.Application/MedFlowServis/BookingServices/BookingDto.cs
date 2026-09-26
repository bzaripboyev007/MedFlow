namespace MedFlow.Application.MedFlowServis.BookingServices;

public class BookingDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string PatientType { get; set; } = null!;
    public string ServiceType { get; set; } = null!;
    public string? Address { get; set; }
    public string? Landmark { get; set; }
    public int NurseId { get; set; }
    public string NurseName { get; set; } = null!;
    public string BookingDate { get; set; } = null!;
    public string BookingTime { get; set; } = null!;
    public string? PrescriptionFile { get; set; }
    public string? PrescriptionUrl { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
