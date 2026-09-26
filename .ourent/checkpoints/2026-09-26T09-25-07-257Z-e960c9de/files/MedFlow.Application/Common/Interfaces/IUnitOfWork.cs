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
    ITelegramLinkRepository TelegramLinks { get; }
    IOtpCodeRepository OtpCodes { get; }
    IBookingRepository Bookings { get; }

    /// <summary>
    /// Amalni bitta DB tranzaksiyasi ichida bajaradi: xatolik bo'lsa rollback qilinadi.
    /// Bir nechta SaveChangesAsync orasida yarim holat qolib ketishining oldini oladi.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
