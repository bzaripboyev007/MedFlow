using MedFlow.Domain.Entities;

namespace MedFlow.Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken, long>
{
}
