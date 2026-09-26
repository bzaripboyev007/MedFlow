using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class PatientMedicineWalletConfiguration : IEntityTypeConfiguration<PatientMedicineWallet>
{
    public void Configure(EntityTypeBuilder<PatientMedicineWallet> builder)
    {
        builder.HasOne(w => w.Patient)
            .WithMany(p => p.WalletEntries)
            .HasForeignKey(w => w.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Medicine)
            .WithMany(m => m.WalletEntries)
            .HasForeignKey(w => w.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.PatientId, w.MedicineId });
    }
}
