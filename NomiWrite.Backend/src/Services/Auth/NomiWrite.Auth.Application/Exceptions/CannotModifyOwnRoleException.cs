namespace NomiWrite.Auth.Application.Exceptions;

public class CannotModifyOwnRoleException : Exception
{
    public CannotModifyOwnRoleException()
        : base("Admins cannot change their own role.")
    {
    }
}