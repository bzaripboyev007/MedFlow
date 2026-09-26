using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class CourseDtoConfig : Profile
{
    public CourseDtoConfig()
    {
        CreateMap<TreatmentCourse, CourseDto>()
            .ForMember(x => x.PatientName, x => x.MapFrom(ent => ent.Patient.FullName))
            .ForMember(x => x.PrimaryNurseName, x => x.MapFrom(ent => ent.PrimaryNurse.FullName))
            .ForMember(x => x.ServiceTypeName, x => x.MapFrom(ent => ent.ServiceType.Name))
            .ForMember(x => x.PricePerSession, x => x.MapFrom(ent => ent.ServiceType.PricePerSession))
            .ForMember(x => x.ProgressText, x => x.MapFrom(ent => $"{ent.CompletedSessions}/{ent.TotalSessions} muolaja bajarildi"))
            .ForMember(x => x.Sessions, x => x.MapFrom(ent => ent.Sessions.OrderBy(s => s.SeqNo)));
    }
}
