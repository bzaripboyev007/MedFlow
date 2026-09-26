using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Payment;

public class PaymentRepository : BaseEntityRepository<Domain.Entities.Payment, long>, IPaymentRepository
{
    public PaymentRepository(MedFlowDbContext context) : base(context)
    {
    }
}
