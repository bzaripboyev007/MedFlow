using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasOne(s => s.Course)
            .WithMany(c => c.Sessions)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Nurse)
            .WithMany(n => n.Sessions)
            .HasForeignKey(s => s.NurseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.NurseId, s.ScheduledAt });
    }
}
