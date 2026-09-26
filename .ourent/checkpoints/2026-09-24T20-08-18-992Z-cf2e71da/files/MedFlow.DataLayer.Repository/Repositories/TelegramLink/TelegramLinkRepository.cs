using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.TelegramLink;

public class TelegramLinkRepository : BaseEntityRepository<Domain.Entities.TelegramLink, long>, ITelegramLinkRepository
{
    public TelegramLinkRepository(MedFlowDbContext context) : base(context)
    {
    }
}
