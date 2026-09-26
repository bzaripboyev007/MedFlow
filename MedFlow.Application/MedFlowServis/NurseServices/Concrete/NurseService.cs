using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.NurseServices.Concrete;

public class NurseService : StatusGenericHandler, INurseService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly INurseAvailabilityService _availability;
    private readonly IMapper _mapper;

    public NurseService(IUnitOfWork uow, IPasswordHasher passwordHasher, INurseAvailabilityService availability, IMapper mapper)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _availability = availability;
        _mapper = mapper;
    }

    public async Task<NurseDto?> CreateAsync(CreateNurseRequest request, CancellationToken ct = default)
    {
        // Telefon raqamni yagona E.164 ko'rinishga keltiramiz ("+998901234567") va bazadagi
        // eski ("+"siz) yozuvlar bilan ham solishtiramiz — X-5 dagi normalizatsiyaning aynan
        // shu naqshi hamshira yaratish oqimida yetishmayotgan edi (X-16). Aks holda bemor
        // "+998901234567" bilan ro'yxatdan o'tgan bo'lsa, "998901234567" bilan hamshira
        // yaratish unique index'ga urilib 500 (DbUpdateException) berardi.
        var phoneNumber = PhoneNumberNormalizer.Normalize(request.Phone);
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.Phone);

        if (phoneNumber.Length == 0)
        {
            AddError("Telefon raqamini kiriting.");
            return null;
        }

        if (await _uow.Users.Query().AnyAsync(u => phoneForms.Contains(u.PhoneNumber), ct))
        {
            AddError("Bu telefon raqami bilan foydalanuvchi allaqachon ro'yxatdan o'tgan.");
            return null;
        }

        var nurse = new Nurse
        {
            FullName = request.FullName,
            Type = request.Type,
            Gender = request.Gender,
            Phone = phoneNumber,
            Specialization = request.Specialization,
            ExperienceStartDate = request.ExperienceStartDate,
            Rating = 0,
            PricePerVisit = request.PricePerVisit > 0 ? request.PricePerVisit : BookingConstants.DefaultNurseVisitPrice,
            IsActive = true,
            HiredAt = DateTimeOffset.UtcNow
        };
        await _uow.Nurses.AddAsync(nurse, ct);
        await _uow.SaveChangesAsync(ct);

        var user = new User
        {
            Role = request.Type == NurseType.Pediatric ? UserRole.PediatricNurse : UserRole.AdultNurse,
            FullName = request.FullName,
            PhoneNumber = phoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            LinkedNurseId = nurse.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _uow.Users.AddAsync(user, ct);

        await _uow.SaveChangesAsync(ct);

        return await ToDtoAsync(nurse, ct);
    }

    public async Task<NurseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var nurse = await _uow.Nurses.GetByIdAsync(id, ct);    
        if (nurse is null)
        {
            AddError($"{nameof(Nurse)} (Id={id}) topilmadi.");
            return null;
        }

        return await ToDtoAsync(nurse, ct);
    }

    public async Task<List<NurseDto>> ListAsync(NurseType? type, Gender? gender, CancellationToken ct = default)
    {
        var query = _uow.Nurses.Query();
        if (type is { } t) query = query.Where(n => n.Type == t);
        if (gender is { } g) query = query.Where(n => n.Gender == g);

        var nurses = await query.OrderBy(n => n.FullName).ToListAsync(ct);
        var result = new List<NurseDto>();
        foreach (var nurse in nurses)
        {
            result.Add(await ToDtoAsync(nurse, ct));
        }

        return result;
    }

    public async Task<List<PublicNurseDto>> ListPublicAsync(NurseType? type, Gender? gender, CancellationToken ct = default)
    {
        var query = _uow.Nurses.Query().Where(n => n.IsActive);
        if (type is { } t) query = query.Where(n => n.Type == t);
        if (gender is { } g) query = query.Where(n => n.Gender == g);

        var nurses = await query.OrderBy(n => n.FullName).ToListAsync(ct);
        return _mapper.Map<List<PublicNurseDto>>(nurses);
    }

    public async Task<NurseDto?> UpdateAsync(int id, UpdateNurseRequest request, CancellationToken ct = default)
    {
        var nurse = await _uow.Nurses.GetByIdAsync(id, ct);
        if (nurse is null)
        {
            AddError($"{nameof(Nurse)} (Id={id}) topilmadi.");
            return null;
        }

        nurse.FullName = request.FullName;
        nurse.Phone = PhoneNumberNormalizer.Normalize(request.Phone);
        nurse.Specialization = request.Specialization;
        nurse.IsActive = request.IsActive;
        nurse.Rating = request.Rating;
        nurse.PricePerVisit = request.PricePerVisit;

        _uow.Nurses.Update(nurse);
        await _uow.SaveChangesAsync(ct);

        return await ToDtoAsync(nurse, ct);
    }

    public async Task<List<BusySlotDto>> GetAvailabilityAsync(int nurseId, DateTime date, CancellationToken ct = default)
    {
        var nurse = await _uow.Nurses.GetByIdAsync(nurseId, ct);
        if (nurse is null)
        {
            AddError($"{nameof(Nurse)} (Id={nurseId}) topilmadi.");
            return new List<BusySlotDto>();
        }

        // Ilgari bu yerda faqat seanslar hisobga olinardi, shuning uchun band bronli vaqt
        // ommaviy endpoint orqali "bo'sh" deb qaytarilardi (X-8 ning qolgan qismi).
        // Endi band vaqtlar yagona manbadan (NurseAvailabilityService) olinadi: seans + faol bron.
        var busySlots = await _availability.GetBusySlotsAsync(nurseId, date, ct);

        return busySlots
            .Select(slot => new BusySlotDto { From = slot.From, To = slot.To })
            .ToList();
    }

    private async Task<NurseDto> ToDtoAsync(Nurse n, CancellationToken ct)
    {
        var completedCourses = await _uow.Courses.Query()
            .CountAsync(c => c.PrimaryNurseId == n.Id && c.Status == CourseStatus.Done, ct);

        var dto = _mapper.Map<NurseDto>(n);
        dto.CompletedCoursesCount = completedCourses;
        return dto;
    }
}
