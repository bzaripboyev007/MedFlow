using AutoMapper;
using MedFlow.Domain.Entities;

namespace MedFlow.Application.MedFlowServis.WalletServices;

public class WalletEntryDtoConfig : Profile
{
    public WalletEntryDtoConfig()
    {
        CreateMap<PatientMedicineWallet, WalletEntryDto>()
            .ForMember(x => x.MedicineName, x => x.MapFrom(ent => ent.Medicine.Name));
    }
}
