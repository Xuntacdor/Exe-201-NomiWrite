using NomiWrite.Auth.Application.Interfaces;

namespace NomiWrite.Auth.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool VerifyPassword(string password, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}