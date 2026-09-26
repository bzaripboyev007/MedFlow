using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.User;

public class UserRepository : BaseEntityRepository<Domain.Entities.User, long>, IUserRepository
{
    public UserRepository(MedFlowDbContext context) : base(context)
    {
    }
}
