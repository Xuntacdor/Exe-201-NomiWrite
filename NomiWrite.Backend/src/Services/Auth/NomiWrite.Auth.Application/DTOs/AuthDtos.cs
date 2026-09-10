using NomiWrite.Auth.Domain.Enums;

namespace NomiWrite.Auth.Application.DTOs;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class VerifyEmailRequestDto
{
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GoogleLoginRequestDto
{
    public string IdToken { get; set; } = string.Empty;
}