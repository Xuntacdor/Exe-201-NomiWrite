namespace NomiWrite.User.Application.Exceptions;

public class ProfileNotFoundException : Exception
{
    public ProfileNotFoundException(Guid userId)
        : base($"User profile for user '{userId}' was not found.")
    {
    }
}
