using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Exceptions;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Auth;

namespace NomiWrite.Auth.Application.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IAuthDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthService(
        IAuthDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = request.Email.Trim().ToLowerInvariant();

        var emailInUse = await _dbContext.Users.AnyAsync(u => u.Email == email);
        if (emailInUse)
            throw new UserAlreadyExistsException(email);

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.Student,
            IsEmailVerified = false
        };

        var (accessToken, accessTokenExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(user.Id, now);

        _dbContext.Users.Add(user);
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id, user.Email, user.FullName));

        return ToResponse(user, accessToken, refreshToken, accessTokenExpiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new InvalidCredentialsException();

        var passwordVerified = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordVerified)
            throw new InvalidCredentialsException();

        var now = DateTime.UtcNow;
        var (accessToken, accessTokenExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(user.Id, now);

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return ToResponse(user, accessToken, refreshToken, accessTokenExpiresAt);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new InvalidRefreshTokenException();

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken is null)
            throw new InvalidRefreshTokenException();

        if (storedToken.IsRevoked)
        {
            await RevokeTokenFamilyAsync(storedToken.UserId);
            throw new InvalidRefreshTokenException();
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidRefreshTokenException();

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == storedToken.UserId)
            ?? throw new InvalidRefreshTokenException();

        var now = DateTime.UtcNow;
        var (accessToken, accessTokenExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = CreateRefreshToken(user.Id, now);

        storedToken.IsRevoked = true;

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        return ToResponse(user, accessToken, newRefreshToken, accessTokenExpiresAt);
    }

    public async Task LogoutAsync(Guid userId)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        if (activeTokens.Count == 0)
            return;

        foreach (var token in activeTokens)
            token.IsRevoked = true;

        await _dbContext.SaveChangesAsync();
    }

    private async Task RevokeTokenFamilyAsync(Guid userId)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        if (activeTokens.Count == 0)
            return;

        foreach (var token in activeTokens)
            token.IsRevoked = true;

        await _dbContext.SaveChangesAsync();
    }

    private RefreshToken CreateRefreshToken(Guid userId, DateTime now)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = _jwtTokenService.GenerateRefreshToken(),
            ExpiresAt = now.Add(RefreshTokenLifetime),
            IsRevoked = false
        };
    }

    private static AuthResponseDto ToResponse(
        User user,
        string accessToken,
        RefreshToken refreshToken,
        DateTime accessTokenExpiresAt)
    {
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = accessTokenExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}