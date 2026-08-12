using MedFlow.Domain.Enums;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.NurseServices.Concrete;

public interface INurseService : IStatusGeneric
{
    Task<NurseDto?> CreateAsync(CreateNurseRequest request, CancellationToken ct = default);
    Task<NurseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<NurseDto>> ListAsync(NurseType? type, Gender? gender, CancellationToken ct = default);
    Task<NurseDto?> UpdateAsync(int id, UpdateNurseRequest request, CancellationToken ct = default);
    Task<List<BusySlotDto>> GetAvailabilityAsync(int nurseId, DateTime date, CancellationToken ct = default);
}
