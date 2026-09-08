namespace NomiWrite.Auth.Application.Exceptions;

public class AccountDeactivatedException : Exception
{
    public AccountDeactivatedException()
        : base("This account has been deactivated")
    {
    }
}