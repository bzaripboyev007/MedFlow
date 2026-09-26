using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Prescription;

public class PrescriptionRepository : BaseEntityRepository<Domain.Entities.Prescription, long>, IPrescriptionRepository
{
    public PrescriptionRepository(MedFlowDbContext context) : base(context)
    {
    }
}
