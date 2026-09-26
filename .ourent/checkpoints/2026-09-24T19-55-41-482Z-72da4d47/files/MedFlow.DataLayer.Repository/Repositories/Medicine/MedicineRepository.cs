using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Medicine;

public class MedicineRepository : BaseEntityRepository<Domain.Entities.Medicine, int>, IMedicineRepository
{
    public MedicineRepository(MedFlowDbContext context) : base(context)
    {
    }
}
