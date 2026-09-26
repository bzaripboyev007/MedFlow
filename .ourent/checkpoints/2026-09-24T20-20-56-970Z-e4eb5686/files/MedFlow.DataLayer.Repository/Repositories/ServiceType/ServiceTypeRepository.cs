using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.ServiceType;

public class ServiceTypeRepository : BaseEntityRepository<Domain.Entities.ServiceType, int>, IServiceTypeRepository
{
    public ServiceTypeRepository(MedFlowDbContext context) : base(context)
    {
    }
}
