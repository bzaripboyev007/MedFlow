using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.PatientServices;

public class PatientDtoConfig : Profile
{
    public PatientDtoConfig()
    {
        CreateMap<Patient, PatientDto>();
    }
}
