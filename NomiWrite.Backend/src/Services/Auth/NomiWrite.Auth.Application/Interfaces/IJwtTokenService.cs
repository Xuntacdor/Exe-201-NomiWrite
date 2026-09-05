using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}