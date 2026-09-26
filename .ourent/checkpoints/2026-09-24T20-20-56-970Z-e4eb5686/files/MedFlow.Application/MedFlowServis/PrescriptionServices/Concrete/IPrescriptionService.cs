using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PrescriptionServices.Concrete;

public interface IPrescriptionService : IStatusGeneric
{
    Task<PrescriptionDto?> UploadAsync(long patientId, Stream content, string fileName, string contentType, CancellationToken ct = default);
    Task<PrescriptionDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<PrescriptionDto>> ListByPatientAsync(long patientId, CancellationToken ct = default);
    Task<PrescriptionDto?> VerifyAsync(long id, int nurseId, CancellationToken ct = default);
}
