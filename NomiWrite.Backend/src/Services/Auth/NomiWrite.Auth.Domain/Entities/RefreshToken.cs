using NomiWrite.Auth.Domain.Common;

namespace NomiWrite.Auth.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? ReplacedByToken { get; set; }
    public Guid UserId { get; set; }

    // Navigation
    public User? User { get; set; }
}
