using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Session;

public class SessionRepository : BaseEntityRepository<Domain.Entities.Session, long>, ISessionRepository
{
    public SessionRepository(MedFlowDbContext context) : base(context)
    {
    }
}
