using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.DataLayer.Repository.Persistence;

public class MedFlowDbContext : DbContext
{
    public MedFlowDbContext(DbContextOptions<MedFlowDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Nurse> Nurses => Set<Nurse>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<TreatmentCourse> Courses => Set<TreatmentCourse>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<PatientMedicineWallet> WalletEntries => Set<PatientMedicineWallet>();
    public DbSet<MedicineIssue> MedicineIssues => Set<MedicineIssue>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
