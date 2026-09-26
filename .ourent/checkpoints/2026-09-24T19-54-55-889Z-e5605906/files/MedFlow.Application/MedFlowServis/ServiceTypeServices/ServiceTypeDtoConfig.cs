using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.ServiceTypeServices;

public class ServiceTypeDtoConfig : Profile
{
    public ServiceTypeDtoConfig()
    {
        CreateMap<ServiceType, ServiceTypeDto>();
    }
}
