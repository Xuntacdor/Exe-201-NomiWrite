namespace NomiWrite.Auth.Application.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public string Email { get; }

    public UserAlreadyExistsException(string email)
        : base($"A user with the email '{email}' already exists.")
    {
        Email = email;
    }
}