using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class CourseSummaryDtoConfig : Profile
{
    public CourseSummaryDtoConfig()
    {
        CreateMap<TreatmentCourse, CourseSummaryDto>()
            .ForMember(x => x.PatientName, x => x.MapFrom(ent => ent.Patient.FullName))
            .ForMember(x => x.PrimaryNurseName, x => x.MapFrom(ent => ent.PrimaryNurse.FullName));
    }
}
