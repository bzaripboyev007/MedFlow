using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.MedicineIssue;

public class MedicineIssueRepository : BaseEntityRepository<Domain.Entities.MedicineIssue, long>, IMedicineIssueRepository
{
    public MedicineIssueRepository(MedFlowDbContext context) : base(context)
    {
    }
}
