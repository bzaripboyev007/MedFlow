using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.SessionServices;

public class SessionDtoConfig : Profile
{
    public SessionDtoConfig()
    {
        CreateMap<Session, SessionDto>()
            .ForMember(x => x.NurseName, x => x.MapFrom(ent => ent.Nurse.FullName));
    }
}
