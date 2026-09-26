using System.Globalization;
using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.BookingServices.Concrete;

public class BookingService : StatusGenericHandler, IBookingService
{
    private readonly IUnitOfWork _uow;
    private readonly INurseAvailabilityService _availability;
    private readonly IMapper _mapper;

    public BookingService(IUnitOfWork uow, INurseAvailabilityService availability, IMapper mapper)
    {
        _uow = uow;
        _availability = availability;
        _mapper = mapper;
    }

    public async Task<BookingDto?> CreateAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var phone = request.Phone?.Trim() ?? string.Empty;
        var patientType = request.PatientType?.Trim().ToLowerInvariant() ?? string.Empty;
        var serviceType = request.ServiceType?.Trim().ToLowerInvariant() ?? string.Empty;
        var address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        if (fullName.Length == 0)
        {
            AddError("Ism va familiyani kiriting.");
            return null;
        }

        if (phone.Length < 9)
        {
            AddError("Telefon raqamini to'g'ri kiriting.");
            return null;
        }

        if (patientType != BookingConstants.PatientTypeAdult && patientType != BookingConstants.PatientTypeChild)
        {
            AddError("Bemor toifasi noto'g'ri (adult yoki child bo'lishi kerak).");
            return null;
        }

        if (serviceType != BookingConstants.ServiceTypeHome && serviceType != BookingConstants.ServiceTypeClinic)
        {
            AddError("Xizmat turi noto'g'ri (home yoki clinic bo'lishi kerak).");
            return null;
        }

        if (serviceType == BookingConstants.ServiceTypeHome && (address is null || address.Length < 5))
        {
            AddError("Uyga chaqirish uchun aniq manzilni kiriting.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.BookingDate))
        {
            AddError("Bron kunini tanlang.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.BookingTime))
        {
            AddError("Bron vaqtini tanlang.");
            return null;
        }

        // Sana/vaqt formatini qat'iy tekshiramiz va normallashtirilgan ko'rinishda saqlaymiz (X-13).
        // Aks holda "2026-13-45" kabi qiymat nvarchar(10) ustunga tushib, keyinchalik
        // NurseAvailabilityService solishtiruvini buzardi.
        if (!DateOnly.TryParseExact(request.BookingDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var bookingDate))
        {
            AddError("Bron kuni 'yyyy-MM-dd' formatida bo'lishi kerak.");
            return null;
        }

        if (!TimeOnly.TryParseExact(request.BookingTime.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var bookingTime))
        {
            AddError("Bron vaqti 'HH:mm' formatida bo'lishi kerak.");
            return null;
        }

        var nurse = await _uow.Nurses.GetByIdAsync(request.NurseId, ct);
        if (nurse is null)
        {
            AddError($"{nameof(Nurse)} (Id={request.NurseId}) topilmadi.");
            return null;
        }

        if (!nurse.IsActive)
        {
            AddError("Tanlangan hamshira hozircha faol emas. Boshqa hamshirani tanlang.");
            return null;
        }

        // Bron yaratishdan oldin hamshira shu vaqtda bo'sh ekaniga ishonch hosil qilamiz:
        // mavjud seanslar (kurslar) va boshqa bronlar hisobga olinadi (X-8).
        var slot = new DateTimeOffset(bookingDate.ToDateTime(bookingTime), TimeSpan.Zero);
        if (!await _availability.IsAvailableForAllAsync(nurse.Id, new[] { slot }, ct))
        {
            AddError("Tanlangan hamshira bu vaqtda band. Boshqa vaqtni tanlang.");
            return null;
        }

        var visitPrice = nurse.PricePerVisit > 0 ? nurse.PricePerVisit : BookingConstants.DefaultNurseVisitPrice;
        var totalPrice = visitPrice + (serviceType == BookingConstants.ServiceTypeHome ? BookingConstants.HomeVisitFee : 0m);

        var booking = new Booking
        {
            FullName = fullName,
            Phone = phone,
            PatientType = patientType,
            ServiceType = serviceType,
            Address = serviceType == BookingConstants.ServiceTypeHome ? address : null,
            Landmark = serviceType == BookingConstants.ServiceTypeHome
                ? (string.IsNullOrWhiteSpace(request.Landmark) ? null : request.Landmark.Trim())
                : null,
            NurseId = nurse.Id,
            BookingDate = bookingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            BookingTime = bookingTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            PrescriptionFile = string.IsNullOrWhiteSpace(request.PrescriptionFile) ? null : request.PrescriptionFile.Trim(),
            PrescriptionUrl = string.IsNullOrWhiteSpace(request.PrescriptionUrl) ? null : request.PrescriptionUrl,
            Status = BookingConstants.StatusNew,
            TotalPrice = totalPrice,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.Bookings.AddAsync(booking, ct);
        await _uow.SaveChangesAsync(ct);

        booking.Nurse = nurse;
        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<List<BookingDto>> ListAsync(int? nurseId = null, CancellationToken ct = default)
    {
        var query = _uow.Bookings.Query().Include(b => b.Nurse).AsQueryable();

        if (nurseId is { } id)
        {
            query = query.Where(b => b.NurseId == id);
        }

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        return _mapper.Map<List<BookingDto>>(bookings);
    }

    public async Task<BookingDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.Query()
            .Include(b => b.Nurse)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            AddError($"{nameof(Booking)} (Id={id}) topilmadi.");
            return null;
        }

        return _mapper.Map<BookingDto>(booking);
    }

    public async Task<BookingDto?> UpdateStatusAsync(long id, string status, CancellationToken ct = default)
    {
        var newStatus = status?.Trim() ?? string.Empty;

        if (!BookingConstants.AllowedStatuses.Contains(newStatus))
        {
            AddError($"'{newStatus}' holati noto'g'ri. Ruxsat etilganlar: {string.Join(", ", BookingConstants.AllowedStatuses)}.");
            return null;
        }

        var booking = await _uow.Bookings.Query()
            .Include(b => b.Nurse)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            AddError($"{nameof(Booking)} (Id={id}) topilmadi.");
            return null;
        }

        booking.Status = newStatus;
        _uow.Bookings.Update(booking);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<BookingDto>(booking);
    }
}
