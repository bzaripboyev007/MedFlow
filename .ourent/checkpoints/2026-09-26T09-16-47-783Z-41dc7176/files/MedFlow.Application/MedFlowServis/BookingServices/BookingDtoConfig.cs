using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.BookingServices;

public class BookingDtoConfig : Profile
{
    public BookingDtoConfig()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(x => x.NurseName, x => x.MapFrom(ent => ent.Nurse.FullName));
    }
}
