using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.PhoneNumber).IsUnique();

        builder.HasOne(u => u.LinkedPatient)
            .WithMany()
            .HasForeignKey(u => u.LinkedPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.LinkedNurse)
            .WithMany()
            .HasForeignKey(u => u.LinkedNurseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
