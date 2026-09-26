namespace MedFlow.Application.MedFlowServis.PrescriptionServices;

public class PrescriptionDto
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string FileUrl { get; set; } = null!;
    public int? VerifiedByNurseId { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
