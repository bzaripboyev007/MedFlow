using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.PatientMedicineWallet;

public class PatientMedicineWalletRepository : BaseEntityRepository<Domain.Entities.PatientMedicineWallet, long>, IPatientMedicineWalletRepository
{
    public PatientMedicineWalletRepository(MedFlowDbContext context) : base(context)
    {
    }
}
