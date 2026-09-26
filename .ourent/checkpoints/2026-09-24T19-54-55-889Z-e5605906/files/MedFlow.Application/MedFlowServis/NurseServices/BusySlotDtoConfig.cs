using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.NurseServices;

public class BusySlotDtoConfig : Profile
{
    public BusySlotDtoConfig()
    {
        CreateMap<Session, BusySlotDto>()
            .ForMember(x => x.From, x => x.MapFrom(ent => ent.ScheduledAt))
            .ForMember(x => x.To, x => x.MapFrom(ent => ent.ScheduledAt.AddMinutes(SchedulingConstants.DefaultSessionDurationMinutes)));
    }
}
