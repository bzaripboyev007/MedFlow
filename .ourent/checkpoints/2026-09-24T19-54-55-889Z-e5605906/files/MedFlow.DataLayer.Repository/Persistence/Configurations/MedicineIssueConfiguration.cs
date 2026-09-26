using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class MedicineIssueConfiguration : IEntityTypeConfiguration<MedicineIssue>
{
    public void Configure(EntityTypeBuilder<MedicineIssue> builder)
    {
        builder.HasOne(i => i.Medicine)
            .WithMany(m => m.Issues)
            .HasForeignKey(i => i.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Nurse)
            .WithMany()
            .HasForeignKey(i => i.NurseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Session)
            .WithMany(s => s.MedicineIssues)
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
