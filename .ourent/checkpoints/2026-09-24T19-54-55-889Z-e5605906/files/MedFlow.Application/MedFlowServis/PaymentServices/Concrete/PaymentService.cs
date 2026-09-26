using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.PaymentServices.Concrete;

public class PaymentService : StatusGenericHandler, IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public PaymentService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PaymentDto?> CreateForCourseAsync(long courseId, PaymentMethod method, CancellationToken ct = default)
    {
        var course = await _uow.Courses.Query()
            .Include(c => c.ServiceType)
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);

        if (course is null)
        {
            AddError($"{nameof(TreatmentCourse)} (Id={courseId}) topilmadi.");
            return null;
        }

        if (course.Status != CourseStatus.Done)
        {
            AddError("Kurs hali yakunlanmagan - to'lov faqat kurs 'Done' bo'lgach yaratiladi.");
            return null;
        }

        var alreadyPaid = await _uow.Payments.Query().AnyAsync(p => p.CourseId == courseId, ct);
        if (alreadyPaid)
        {
            AddError("Bu kurs uchun to'lov allaqachon yaratilgan.");
            return null;
        }

        var serviceAmount = course.ServiceType.PricePerSession * course.TotalSessions;

        var medicineAmount = await _uow.MedicineIssues.Query()
            .Where(i => i.Session.CourseId == courseId)
            .SumAsync(i => i.PriceAtIssue, ct);

        var transportAmount = course.LocationType == LocationType.Home ? PricingConstants.HomeVisitTransportFee : 0m;

        var payment = new Payment
        {
            PatientId = course.PatientId,
            CourseId = course.Id,
            ServiceAmount = serviceAmount,
            MedicineAmount = medicineAmount,
            TransportAmount = transportAmount,
            TotalAmount = serviceAmount + medicineAmount + transportAmount,
            Method = method,
            PaidAt = DateTimeOffset.UtcNow
        };

        await _uow.Payments.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> GetByCourseAsync(long courseId, CancellationToken ct = default)
    {
        var payment = await _uow.Payments.Query().FirstOrDefaultAsync(p => p.CourseId == courseId, ct);
        if (payment is null)
        {
            AddError($"{nameof(Payment)} (Id={courseId}) topilmadi.");
            return null;
        }

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<List<PaymentDto>> ListByPatientAsync(long patientId, CancellationToken ct = default)
    {
        var payments = await _uow.Payments.Query()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(ct);

        return _mapper.Map<List<PaymentDto>>(payments);
    }
}
