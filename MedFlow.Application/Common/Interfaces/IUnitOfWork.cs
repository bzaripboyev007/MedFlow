using MedFlow.Application.Common.Interfaces.Repositories;

namespace MedFlow.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IPatientRepository Patients { get; }
    INurseRepository Nurses { get; }
    IPrescriptionRepository Prescriptions { get; }
    ITreatmentCourseRepository Courses { get; }
    ISessionRepository Sessions { get; }
    IMedicineRepository Medicines { get; }
    IPatientMedicineWalletRepository WalletEntries { get; }
    IMedicineIssueRepository MedicineIssues { get; }
    IServiceTypeRepository ServiceTypes { get; }
    IPaymentRepository Payments { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
