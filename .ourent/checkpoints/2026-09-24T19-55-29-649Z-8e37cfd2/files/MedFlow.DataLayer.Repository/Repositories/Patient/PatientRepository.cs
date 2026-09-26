using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Patient;

public class PatientRepository : BaseEntityRepository<Domain.Entities.Patient, long>, IPatientRepository
{
    public PatientRepository(MedFlowDbContext context) : base(context)
    {
    }
}
