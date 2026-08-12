using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedFlow.DataLayer.Repository.Persistence.Configurations;

public class TreatmentCourseConfiguration : IEntityTypeConfiguration<TreatmentCourse>
{
    public void Configure(EntityTypeBuilder<TreatmentCourse> builder)
    {
        builder.Property(c => c.PreferredTimesMinutes).HasColumnType("integer[]");

        builder.HasOne(c => c.Patient)
            .WithMany(p => p.Courses)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Prescription)
            .WithMany(p => p.Courses)
            .HasForeignKey(c => c.PrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PrimaryNurse)
            .WithMany(n => n.PrimaryCourses)
            .HasForeignKey(c => c.PrimaryNurseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ServiceType)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
