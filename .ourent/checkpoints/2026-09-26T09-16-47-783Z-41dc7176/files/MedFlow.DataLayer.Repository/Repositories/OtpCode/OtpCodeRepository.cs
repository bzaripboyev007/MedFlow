using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.OtpCode;

public class OtpCodeRepository : BaseEntityRepository<Domain.Entities.OtpCode, long>, IOtpCodeRepository
{
    public OtpCodeRepository(MedFlowDbContext context) : base(context)
    {
    }
}
