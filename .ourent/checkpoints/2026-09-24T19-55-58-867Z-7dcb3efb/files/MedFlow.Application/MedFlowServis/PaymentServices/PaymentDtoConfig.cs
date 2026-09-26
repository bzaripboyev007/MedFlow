using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.PaymentServices;

public class PaymentDtoConfig : Profile
{
    public PaymentDtoConfig()
    {
        CreateMap<Payment, PaymentDto>();
    }
}
