using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using MedFlow.DataLayer.Repository.Repositories.Base;

namespace MedFlow.DataLayer.Repository.Repositories.Booking;

public class BookingRepository : BaseEntityRepository<Domain.Entities.Booking, long>, IBookingRepository
{
    public BookingRepository(MedFlowDbContext context) : base(context)
    {
    }
}
