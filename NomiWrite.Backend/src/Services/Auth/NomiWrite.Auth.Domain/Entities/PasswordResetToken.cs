using NomiWrite.Auth.Domain.Common;

namespace NomiWrite.Auth.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public Guid UserId { get; set; }

    public User? User { get; set; }
}
