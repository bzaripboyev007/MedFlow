using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.PrescriptionServices;

public class PrescriptionDtoConfig : Profile
{
    public PrescriptionDtoConfig()
    {
        CreateMap<Prescription, PrescriptionDto>();
    }
}
