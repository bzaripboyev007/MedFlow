using AutoMapper;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.ServiceTypeServices.Concrete;

public class ServiceTypeService : StatusGenericHandler, IServiceTypeService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ServiceTypeService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ServiceTypeDto?> CreateAsync(CreateServiceTypeRequest request, CancellationToken ct = default)
    {
        var serviceType = new ServiceType
        {
            Name = request.Name,
            PricePerSession = request.PricePerSession,
            IsActive = true
        };

        await _uow.ServiceTypes.AddAsync(serviceType, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ServiceTypeDto>(serviceType);
    }

    public async Task<ServiceTypeDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var serviceType = await _uow.ServiceTypes.GetByIdAsync(id, ct);
        if (serviceType is null)
        {
            AddError($"{nameof(ServiceType)} (Id={id}) topilmadi.");
            return null;
        }

        return _mapper.Map<ServiceTypeDto>(serviceType);
    }

    public async Task<List<ServiceTypeDto>> ListAsync(CancellationToken ct = default)
    {
        var serviceTypes = await _uow.ServiceTypes.Query().OrderBy(s => s.Name).ToListAsync(ct);
        return _mapper.Map<List<ServiceTypeDto>>(serviceTypes);
    }
}
