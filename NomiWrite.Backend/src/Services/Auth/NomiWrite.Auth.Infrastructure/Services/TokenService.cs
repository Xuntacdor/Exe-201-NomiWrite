using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Entities;

namespace NomiWrite.Auth.Infrastructure.Services;

/// <summary>
/// JWT token generation service.
/// </summary>
public class TokenService : ITokenService
{
    public string GenerateAccessToken(User user)
    {
        // TODO: Build JWT with claims (userId, email, role), sign with secret
        throw new NotImplementedException();
    }

    public string GenerateRefreshToken()
    {
        // TODO: Generate cryptographically secure random string
        throw new NotImplementedException();
    }
}
