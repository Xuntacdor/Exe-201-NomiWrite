namespace NomiWrite.Auth.Application.Exceptions;

public class AccountBannedException : Exception
{
    public AccountBannedException()
        : base("This account has been banned")
    {
    }
}