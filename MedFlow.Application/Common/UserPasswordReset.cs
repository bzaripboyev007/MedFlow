using MedFlow.Application.Common.Interfaces;
using MedFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.Application.Common;

/// <summary>
/// Admin tomonidan parolni almashtirishning umumiy qismi (bemor va hamshira uchun bir xil).
/// SaveChangesAsync chaqirmaydi - chaqiruvchi saqlaydi.
/// </summary>
public static class UserPasswordReset
{
    public static async Task ApplyAsync(IUnitOfWork uow, IPasswordHasher passwordHasher, User user, string newPassword, CancellationToken ct)
    {
        user.PasswordHash = passwordHasher.Hash(newPassword);

        // PASSWORD ustuni varchar(30): undan uzun parol sig'maydi, shuning uchun bo'sh qoldiriladi.
        user.Password = newPassword.Length <= 30 ? newPassword : null;

        // Yangi parol bilan darhol kira olsin: noto'g'ri urinishlar tufayli qo'yilgan blok ham olinadi.
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        uow.Users.Update(user);

        // Eski sessiyalar yopiladi: eski parol bilan olingan refresh token endi ishlamaydi.
        var activeTokens = await uow.RefreshTokens.Query()
            .Where(r => r.UserId == user.Id && r.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            uow.RefreshTokens.Update(token);
        }
    }
}
