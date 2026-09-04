namespace NomiWrite.Shared.Contracts.Events.Auth;

/// <summary>
/// Published by Auth Service when a user successfully logs in.
/// Consumed by Logging/Activity service.
/// </summary>
public record UserLoggedInEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public DateTime LoggedInAt { get; init; } = DateTime.UtcNow;
}
