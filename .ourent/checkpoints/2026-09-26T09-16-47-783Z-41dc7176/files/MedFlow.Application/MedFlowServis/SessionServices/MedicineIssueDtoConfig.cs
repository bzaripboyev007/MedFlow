using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.SessionServices;

public class MedicineIssueDtoConfig : Profile
{
    public MedicineIssueDtoConfig()
    {
        CreateMap<MedicineIssue, MedicineIssueDto>()
            .ForMember(x => x.MedicineName, x => x.MapFrom(ent => ent.Medicine.Name));
    }
}
