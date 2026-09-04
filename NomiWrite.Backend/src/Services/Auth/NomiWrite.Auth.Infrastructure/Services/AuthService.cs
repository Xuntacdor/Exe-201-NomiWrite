using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Interfaces;
using MassTransit;

namespace NomiWrite.Auth.Infrastructure.Services;

/// <summary>
/// Auth service implementation.
/// Publishes UserRegisteredEvent and UserLoggedInEvent to RabbitMQ via MassTransit.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _publishEndpoint = publishEndpoint;
    }

    public Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // TODO: Hash password, create user, publish UserRegisteredEvent
        throw new NotImplementedException();
    }

    public Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // TODO: Verify credentials, generate tokens, publish UserLoggedInEvent
        throw new NotImplementedException();
    }

    public Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        // TODO: Validate refresh token, rotate, return new tokens
        throw new NotImplementedException();
    }

    public Task LogoutAsync(Guid userId)
    {
        // TODO: Revoke all refresh tokens for user
        throw new NotImplementedException();
    }
}
