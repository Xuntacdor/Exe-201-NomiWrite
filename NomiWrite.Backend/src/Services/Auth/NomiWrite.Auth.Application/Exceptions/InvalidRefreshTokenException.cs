namespace NomiWrite.Auth.Application.Exceptions;

public class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException()
        : base("The refresh token is invalid, expired, or has been revoked.")
    {
    }
}