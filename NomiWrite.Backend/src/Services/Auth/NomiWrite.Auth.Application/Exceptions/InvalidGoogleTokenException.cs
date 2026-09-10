namespace NomiWrite.Auth.Application.Exceptions;

public class InvalidGoogleTokenException : Exception
{
    public InvalidGoogleTokenException()
        : base("The Google authentication token is invalid or has expired.")
    {
    }

    public InvalidGoogleTokenException(string message)
        : base(message)
    {
    }
}
