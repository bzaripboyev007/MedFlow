using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.NurseServices;

public class NurseDtoConfig : Profile
{
    public NurseDtoConfig()
    {
        CreateMap<Nurse, NurseDto>()
            .ForMember(x => x.ExperienceYears, x => x.MapFrom(ent => (int)((DateTime.Today - ent.ExperienceStartDate).TotalDays / 365.25)));

        CreateMap<Nurse, PublicNurseDto>()
            .ForMember(x => x.ExperienceYears, x => x.MapFrom(ent => (int)((DateTime.Today - ent.ExperienceStartDate).TotalDays / 365.25)));
    }
}
