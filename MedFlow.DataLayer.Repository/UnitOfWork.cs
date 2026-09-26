using MedFlow.Application.Common.Interfaces;
using MedFlow.Application.Common.Interfaces.Repositories;
using MedFlow.DataLayer.Repository.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.DataLayer.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly MedFlowDbContext _context;

    private IPatientRepository? _patients;
    private INurseRepository? _nurses;
    private IPrescriptionRepository? _prescriptions;
    private ITreatmentCourseRepository? _courses;
    private ISessionRepository? _sessions;
    private IMedicineRepository? _medicines;
    private IPatientMedicineWalletRepository? _walletEntries;
    private IMedicineIssueRepository? _medicineIssues;
    private IServiceTypeRepository? _serviceTypes;
    private IPaymentRepository? _payments;
    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private ITelegramLinkRepository? _telegramLinks;
    private IOtpCodeRepository? _otpCodes;
    private IBookingRepository? _bookings;

    public UnitOfWork(MedFlowDbContext context)
    {
        _context = context;
    }

    public IPatientRepository Patients => _patients ??= new Repositories.Patient.PatientRepository(_context);
    public INurseRepository Nurses => _nurses ??= new Repositories.Nurse.NurseRepository(_context);
    public IPrescriptionRepository Prescriptions => _prescriptions ??= new Repositories.Prescription.PrescriptionRepository(_context);
    public ITreatmentCourseRepository Courses => _courses ??= new Repositories.TreatmentCourse.TreatmentCourseRepository(_context);
    public ISessionRepository Sessions => _sessions ??= new Repositories.Session.SessionRepository(_context);
    public IMedicineRepository Medicines => _medicines ??= new Repositories.Medicine.MedicineRepository(_context);
    public IPatientMedicineWalletRepository WalletEntries => _walletEntries ??= new Repositories.PatientMedicineWallet.PatientMedicineWalletRepository(_context);
    public IMedicineIssueRepository MedicineIssues => _medicineIssues ??= new Repositories.MedicineIssue.MedicineIssueRepository(_context);
    public IServiceTypeRepository ServiceTypes => _serviceTypes ??= new Repositories.ServiceType.ServiceTypeRepository(_context);
    public IPaymentRepository Payments => _payments ??= new Repositories.Payment.PaymentRepository(_context);
    public IUserRepository Users => _users ??= new Repositories.User.UserRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new Repositories.RefreshToken.RefreshTokenRepository(_context);
    public ITelegramLinkRepository TelegramLinks => _telegramLinks ??= new Repositories.TelegramLink.TelegramLinkRepository(_context);
    public IOtpCodeRepository OtpCodes => _otpCodes ??= new Repositories.OtpCode.OtpCodeRepository(_context);
    public IBookingRepository Bookings => _bookings ??= new Repositories.Booking.BookingRepository(_context);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        var result = await action();
        await transaction.CommitAsync(ct);
        return result;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
