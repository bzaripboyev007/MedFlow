using MedFlow.Domain.Entities;

namespace MedFlow.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(User user);

    /// <summary>Yangi xom refresh token va uning hash'ini yaratadi (xom qiymat mijozga qaytariladi, hash bazaga yoziladi).</summary>
    (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) CreateRefreshToken();

    /// <summary>Mijoz yuborgan xom refresh tokenni bazadagi hash bilan solishtirish uchun.</summary>
    string HashToken(string rawToken);
}
