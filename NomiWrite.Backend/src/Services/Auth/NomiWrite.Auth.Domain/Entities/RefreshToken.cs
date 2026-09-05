using NomiWrite.Auth.Domain.Common;

namespace NomiWrite.Auth.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public Guid UserId { get; set; }

    public User? User { get; set; }
}