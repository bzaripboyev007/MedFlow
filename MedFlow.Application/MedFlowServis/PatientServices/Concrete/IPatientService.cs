using MedFlow.Application.Common;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PatientServices.Concrete;

public interface IPatientService : IStatusGeneric
{
    Task<PatientDto?> CreateAsync(CreatePatientRequest request, CancellationToken ct = default);
    Task<PatientDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<PatientDto>> ListAsync(long? parentId, CancellationToken ct = default);
    Task<PagedResultDto<PatientDto>> SortFilterAsync(PatientSortFilterRequest request, CancellationToken ct = default);
    Task<PatientDto?> UpdateAsync(long id, UpdatePatientRequest request, CancellationToken ct = default);

    /// <summary>Bemorning login hisobi parolini almashtiradi va uning barcha refresh tokenlarini bekor qiladi.</summary>
    Task<bool> ChangePasswordAsync(long patientId, string newPassword, CancellationToken ct = default);

    /// <summary>Resurs darajasidagi RBAC uchun: targetPatientId - callerPatientId'ning o'zimi yoki farzandimi.</summary>
    Task<bool> IsSelfOrChildAsync(long callerPatientId, long targetPatientId, CancellationToken ct = default);
}
