namespace MedFlow.Application.MedFlowServis.BookingServices;

public class UpdateBookingStatusRequest
{
    /// <summary>"Yangi" | "Kelgan" | "Jarayonda" | "Yakunlandi" | "Bekor qilindi".</summary>
    public string Status { get; set; } = null!;
}
