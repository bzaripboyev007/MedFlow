using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Nurse;

public class NurseRepository : BaseEntityRepository<Domain.Entities.Nurse, int>, INurseRepository
{
    public NurseRepository(MedFlowDbContext context) : base(context) 
    {
    }
}
