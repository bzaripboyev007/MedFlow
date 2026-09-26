using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.PharmacyServices;

public class MedicineDtoConfig : Profile
{
    public MedicineDtoConfig()
    {
        CreateMap<Medicine, MedicineDto>()
            .ForMember(x => x.IsLowStock, x => x.MapFrom(ent => ent.StockQty <= ent.MinStockLevel))
            .ForMember(x => x.IsExpiringSoon, x => x.MapFrom(ent => ent.ExpiryDate <= DateTime.UtcNow.Date.AddDays(SchedulingConstants.ExpirySoonThresholdDays)));
    }
}
