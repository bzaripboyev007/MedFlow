using AutoMapper;
using MedFlow.Application.Common;
using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using MedFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.NurseServices.Concrete;

public class NurseService : StatusGenericHandler, INurseService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public NurseService(IUnitOfWork uow, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<NurseDto?> CreateAsync(CreateNurseRequest request, CancellationToken ct = default)
    {
        if (await _uow.Users.Query().AnyAsync(u => u.PhoneNumber == request.Phone, ct))
        {
            AddError("Bu telefon raqami bilan foydalanuvchi allaqachon ro'yxatdan o'tgan.");
            return null;
        }

        var nurse = new Nurse
        {
            FullName = request.FullName,
            Type = request.Type,
            Gender = request.Gender,
            Phone = request.Phone,
            Specialization = request.Specialization,
            ExperienceStartDate = request.ExperienceStartDate,
            Login = request.Login,
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
            PhoneNumber = request.Phone,
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
        nurse.Phone = request.Phone;
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

        var dayStart = new DateTimeOffset(date.Date, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        var busySessions = await _uow.Sessions.Query()
            .Where(s => s.NurseId == nurseId
                        && s.ScheduledAt >= dayStart && s.ScheduledAt < dayEnd
                        && (s.Status == SessionStatus.Pending || s.Status == SessionStatus.Arrived || s.Status == SessionStatus.InProgress))
            .OrderBy(s => s.ScheduledAt)
            .ToListAsync(ct);

        return _mapper.Map<List<BusySlotDto>>(busySessions);
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
