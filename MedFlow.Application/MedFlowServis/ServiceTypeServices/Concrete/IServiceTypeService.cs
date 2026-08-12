using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.ServiceTypeServices.Concrete;

public interface IServiceTypeService : IStatusGeneric
{
    Task<ServiceTypeDto?> CreateAsync(CreateServiceTypeRequest request, CancellationToken ct = default);
    Task<ServiceTypeDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<ServiceTypeDto>> ListAsync(CancellationToken ct = default);
}
