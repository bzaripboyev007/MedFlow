using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.RefreshToken;

public class RefreshTokenRepository : BaseEntityRepository<Domain.Entities.RefreshToken, long>, IRefreshTokenRepository
{
    public RefreshTokenRepository(MedFlowDbContext context) : base(context)
    {
    }
}
