using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasOne(b => b.Nurse)
            .WithMany()
            .HasForeignKey(b => b.NurseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.CreatedAt);
        builder.HasIndex(b => new { b.NurseId, b.BookingDate });
    }
}
