using System.Security.Cryptography;
using FluentValidation;
using Google.Apis.Auth;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Exceptions;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Application.Options;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Auth;

namespace NomiWrite.Auth.Application.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan EmailVerificationTokenLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    private const string ForgotPasswordGenericResponse =
        "If an account with that email exists, a password reset link has been sent.";
    private const string ResendVerificationGenericResponse =
        "If your account requires verification, a verification email has been sent.";

    private readonly IAuthDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<VerifyEmailRequestDto> _verifyEmailValidator;
    private readonly IValidator<ResendVerificationEmailRequestDto> _resendVerificationEmailValidator;
    private readonly IValidator<ForgotPasswordRequestDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordRequestDto> _resetPasswordValidator;
    private readonly IEmailSender _emailSender;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IOptions<GoogleAuthSettings> _googleAuthSettings;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<VerifyEmailRequestDto> verifyEmailValidator,
        IValidator<ResendVerificationEmailRequestDto> resendVerificationEmailValidator,
        IValidator<ForgotPasswordRequestDto> forgotPasswordValidator,
        IValidator<ResetPasswordRequestDto> resetPasswordValidator,
        IEmailSender emailSender,
        IOptions<AppSettings> appSettings,
        IOptions<GoogleAuthSettings> googleAuthSettings,
        IPublishEndpoint publishEndpoint,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _resendVerificationEmailValidator = resendVerificationEmailValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _emailSender = emailSender;
        _appSettings = appSettings;
        _googleAuthSettings = googleAuthSettings;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
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
        var emailVerificationToken = CreateEmailVerificationToken(user.Id, now);

        _dbContext.Users.Add(user);
        _dbContext.RefreshTokens.Add(refreshToken);
        _dbContext.EmailVerificationTokens.Add(emailVerificationToken);
        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id, user.Email, user.FullName));

        // Best-effort email delivery: a failed verification email must not break signup.
        await TrySendVerificationEmailAsync(user.Email, emailVerificationToken.Token);

        return ToResponse(user, accessToken, refreshToken, accessTokenExpiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = request.Email.Trim().ToLowerInvariant();

        // IgnoreQueryFilters: a deactivated (soft-deleted) account must still be found
        // so we can reject it with a clear message instead of "invalid credentials".
        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new InvalidCredentialsException();

        if (user.IsDeleted)
            throw new AccountDeactivatedException();

        if (user.AccountStatus != AccountStatus.Active)
        {
            if (user.AccountStatus == AccountStatus.Banned)
                throw new AccountBannedException();
            throw new AccountDeactivatedException();
        }

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

    public async Task<AuthResultDto> VerifyEmailAsync(VerifyEmailRequestDto request)
    {
        var validationResult = await _verifyEmailValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var storedToken = await _dbContext.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (storedToken is null || storedToken.ExpiresAt <= DateTime.UtcNow)
            return Failure("The verification token is invalid or has expired.");

        // Idempotency: a previously used token for an already-verified account
        // returns success instead of an error, so a repeated call does not fail.
        if (storedToken.IsUsed)
        {
            var alreadyVerified = await _dbContext.Users
                .AnyAsync(u => u.Id == storedToken.UserId && u.IsEmailVerified);

            return alreadyVerified
                ? Success("Email has already been verified.")
                : Failure("The verification token is invalid or has expired.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == storedToken.UserId);
        if (user is null)
            return Failure("The verification token is invalid or has expired.");

        user.IsEmailVerified = true;
        storedToken.IsUsed = true;
        await _dbContext.SaveChangesAsync();

        return Success("Email verified successfully.");
    }

    public async Task<AuthResultDto> ResendVerificationEmailAsync(ResendVerificationEmailRequestDto request, Guid userId)
    {
        var validationResult = await _resendVerificationEmailValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // [Authorize]: the caller can only resend for their own account (userId from JWT sub),
        // which prevents account enumeration and needs no rate-limiting infrastructure.
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        // Always return the same generic message, even if the account cannot be verified further.
        if (user is null || user.IsEmailVerified)
            return Success(ResendVerificationGenericResponse);

        var now = DateTime.UtcNow;

        var previousTokens = await _dbContext.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();
        foreach (var token in previousTokens)
            token.IsUsed = true;

        var newToken = CreateEmailVerificationToken(userId, now);
        _dbContext.EmailVerificationTokens.Add(newToken);
        await _dbContext.SaveChangesAsync();

        await SendVerificationEmailAsync(user.Email, newToken.Token);

        return Success(ResendVerificationGenericResponse);
    }

    public async Task<AuthResultDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var validationResult = await _forgotPasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Always return the same generic message so the response does not reveal
        // whether an account exists for the requested email (prevents enumeration).
        if (user is null)
            return Success(ForgotPasswordGenericResponse);

        var now = DateTime.UtcNow;

        var previousTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync();
        foreach (var token in previousTokens)
            token.IsUsed = true;

        var resetToken = CreatePasswordResetToken(user.Id, now);
        _dbContext.PasswordResetTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync();

        await SendPasswordResetEmailAsync(user.Email, resetToken.Token);

        return Success(ForgotPasswordGenericResponse);
    }

    public async Task<AuthResultDto> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var storedToken = await _dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (storedToken is null || storedToken.IsUsed || storedToken.ExpiresAt <= DateTime.UtcNow)
            return Failure("The reset token is invalid or has expired.");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == storedToken.UserId);
        if (user is null)
            return Failure("The reset token is invalid or has expired.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        storedToken.IsUsed = true;

        // Revoke every existing session so a reset invalidates all pre-reset tokens.
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync();
        foreach (var token in activeTokens)
            token.IsRevoked = true;

        await _dbContext.SaveChangesAsync();

        return Success("Your password has been reset. Please log in again.");
    }

    public async Task<AuthResultDto> DeactivateAccountAsync(Guid userId)
    {
        // IgnoreQueryFilters so an already-deactivated account is still found
        // and we can answer with a clear message instead of "not found".
        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Failure("Account not found.");

        if (user.IsDeleted)
            return Failure("This account has already been deactivated.");

        user.IsDeleted = true;

        // Revoke every existing session alongside the deactivation.
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        foreach (var token in activeTokens)
            token.IsRevoked = true;

        await _dbContext.SaveChangesAsync();

        return Success("Account deactivated successfully.");
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            throw new InvalidGoogleTokenException("The Google ID token is required.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleAuthSettings.Value.ClientId }
            };

            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed");
            throw new InvalidGoogleTokenException("The Google authentication token is invalid or has expired.");
        }

        var email = payload.Email.Trim().ToLowerInvariant();
        var googleId = payload.Subject;
        var now = DateTime.UtcNow;

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        bool isNewUser = false;

        if (user is not null)
        {
            if (user.IsDeleted)
                throw new AccountDeactivatedException();

            if (user.AccountStatus == AccountStatus.Banned)
                throw new AccountBannedException();

            if (user.GoogleId is null)
                user.GoogleId = googleId;

            if (user.AvatarUrl is null && !string.IsNullOrWhiteSpace(payload.Picture))
                user.AvatarUrl = payload.Picture;

            if (!user.IsEmailVerified)
                user.IsEmailVerified = true;
        }
        else
        {
            isNewUser = true;
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = string.IsNullOrWhiteSpace(payload.Name) ? email : payload.Name.Trim(),
                PasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N")),
                GoogleId = googleId,
                AvatarUrl = payload.Picture,
                Role = UserRole.Student,
                IsEmailVerified = true
            };

            _dbContext.Users.Add(user);
        }

        var (accessToken, accessTokenExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(user.Id, now);

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        if (isNewUser)
            await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id, user.Email, user.FullName));

        return ToResponse(user, accessToken, refreshToken, accessTokenExpiresAt);
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

    private EmailVerificationToken CreateEmailVerificationToken(Guid userId, DateTime now)
    {
        return new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = now.Add(EmailVerificationTokenLifetime),
            IsUsed = false
        };
    }

    private PasswordResetToken CreatePasswordResetToken(Guid userId, DateTime now)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = now.Add(PasswordResetTokenLifetime),
            IsUsed = false
        };
    }

    private async Task TrySendVerificationEmailAsync(string email, string token)
    {
        try
        {
            await SendVerificationEmailAsync(email, token);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send verification email to {Email}", email);
        }
    }

    private async Task SendVerificationEmailAsync(string email, string token)
    {
        var verificationLink = BuildLink("/verify-email", token);

        await _emailSender.SendEmailAsync(
            email,
            "Verify your email address",
            $"<p>Welcome to NomiWrite!</p>" +
            $"<p>Please confirm your email address by clicking the link below:</p>" +
            $"<p><a href=\"{verificationLink}\">Verify email</a></p>" +
            $"<p>This link expires in 24 hours. If you did not create an account, please ignore this email.</p>");
    }

    private async Task SendPasswordResetEmailAsync(string email, string token)
    {
        var resetLink = BuildLink("/reset-password", token);

        await _emailSender.SendEmailAsync(
            email,
            "Reset your password",
            $"<p>We received a request to reset your NomiWrite password.</p>" +
            $"<p>Click the link below to choose a new password:</p>" +
            $"<p><a href=\"{resetLink}\">Reset password</a></p>" +
            $"<p>This link expires in 1 hour. If you did not request this, you can safely ignore this email.</p>");
    }

    private string BuildLink(string path, string token)
    {
        var baseUrl = _appSettings.Value.FrontendBaseUrl.TrimEnd('/');
        return $"{baseUrl}{path}?token={Uri.EscapeDataString(token)}";
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static AuthResultDto Success(string message)
        => new() { Success = true, Message = message };

    private static AuthResultDto Failure(string message)
        => new() { Success = false, Message = message };

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