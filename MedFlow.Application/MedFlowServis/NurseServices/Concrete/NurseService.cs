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

        if (!PhoneNumberNormalizer.IsValidUzbek(phoneNumber))
        {
            AddError(PhoneNumberNormalizer.InvalidMessage);
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
        // Nurse va uning login hisobi (User) bitta tranzaksiyada yaratiladi: User saqlanmay
        // qolsa, tizimga kira olmaydigan "orphan" hamshira qolib ketmaydi (X-11 bilan bir xil naqsh).
        await _uow.ExecuteInTransactionAsync(async () =>
        {
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

            return user;
        }, ct);

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

    public async Task<PagedResultDto<NurseDto>> SortFilterAsync(NurseSortFilterRequest request, CancellationToken ct = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _uow.Nurses.Query();

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var pattern = search.ToLower();
            query = query.Where(n => n.FullName.ToLower().Contains(pattern));
        }

        if (request.Type is { } type) query = query.Where(n => n.Type == type);
        if (request.Gender is { } gender) query = query.Where(n => n.Gender == gender);
        if (request.IsActive is { } isActive) query = query.Where(n => n.IsActive == isActive);

        query = (request.SortBy, request.SortDesc) switch
        {
            (NurseSortField.ExperienceStartDate, false) => query.OrderBy(n => n.ExperienceStartDate).ThenBy(n => n.Id),
            (NurseSortField.ExperienceStartDate, true) => query.OrderByDescending(n => n.ExperienceStartDate).ThenBy(n => n.Id),
            (NurseSortField.Rating, false) => query.OrderBy(n => n.Rating).ThenBy(n => n.Id),
            (NurseSortField.Rating, true) => query.OrderByDescending(n => n.Rating).ThenBy(n => n.Id),
            (NurseSortField.PricePerVisit, false) => query.OrderBy(n => n.PricePerVisit).ThenBy(n => n.Id),
            (NurseSortField.PricePerVisit, true) => query.OrderByDescending(n => n.PricePerVisit).ThenBy(n => n.Id),
            (NurseSortField.HiredAt, false) => query.OrderBy(n => n.HiredAt).ThenBy(n => n.Id),
            (NurseSortField.HiredAt, true) => query.OrderByDescending(n => n.HiredAt).ThenBy(n => n.Id),
            (_, true) => query.OrderByDescending(n => n.FullName).ThenBy(n => n.Id),
            _ => query.OrderBy(n => n.FullName).ThenBy(n => n.Id)
        };

        var totalCount = await query.CountAsync(ct);
        var nurses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Yakunlangan kurslar soni sahifadagi barcha hamshiralar uchun bitta so'rovda olinadi.
        var ids = nurses.Select(n => n.Id).ToList();
        var completedCounts = await _uow.Courses.Query()
            .Where(c => ids.Contains(c.PrimaryNurseId) && c.Status == CourseStatus.Done)
            .GroupBy(c => c.PrimaryNurseId)
            .Select(g => new { NurseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.NurseId, x => x.Count, ct);

        var items = _mapper.Map<List<NurseDto>>(nurses);
        foreach (var dto in items)
        {
            dto.CompletedCoursesCount = completedCounts.GetValueOrDefault(dto.Id);
        }

        return new PagedResultDto<NurseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ChangePasswordAsync(int nurseId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < AuthConstants.MinPasswordLength)
        {
            AddError($"Parol kamida {AuthConstants.MinPasswordLength} belgidan iborat bo'lishi kerak.");
            return false;
        }

        if (await _uow.Nurses.GetByIdAsync(nurseId, ct) is null)
        {
            AddError($"{nameof(Nurse)} (Id={nurseId}) topilmadi.");
            return false;
        }

        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.LinkedNurseId == nurseId, ct);
        if (user is null)
        {
            AddError("Bu hamshiraning tizimga kirish hisobi yo'q.");
            return false;
        }

        await UserPasswordReset.ApplyAsync(_uow, _passwordHasher, user, newPassword, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
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

        var phoneNumber = PhoneNumberNormalizer.Normalize(request.Phone);
        var phoneForms = PhoneNumberNormalizer.StorageForms(request.Phone);

        if (!PhoneNumberNormalizer.IsValidUzbek(phoneNumber))
        {
            AddError(PhoneNumberNormalizer.InvalidMessage);
            return null;
        }

        if (await _uow.Users.Query().AnyAsync(u => phoneForms.Contains(u.PhoneNumber) && u.LinkedNurseId != id, ct))
        {
            AddError("Bu telefon raqami bilan foydalanuvchi allaqachon ro'yxatdan o'tgan.");
            return null;
        }

        nurse.FullName = request.FullName;
        nurse.Phone = phoneNumber;
        nurse.Specialization = request.Specialization;
        nurse.IsActive = request.IsActive;
        nurse.Rating = request.Rating;
        nurse.PricePerVisit = request.PricePerVisit;

        _uow.Nurses.Update(nurse);

        // Hamshira tizimga telefon raqami bilan kiradi, shuning uchun login hisobi ham yangilanadi -
        // aks holda raqam o'zgargach u hali ham eski raqam bilan kirishga majbur bo'lardi.
        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.LinkedNurseId == id, ct);
        if (user is not null)
        {
            user.FullName = request.FullName;
            user.PhoneNumber = phoneNumber;
            _uow.Users.Update(user);
        }

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
