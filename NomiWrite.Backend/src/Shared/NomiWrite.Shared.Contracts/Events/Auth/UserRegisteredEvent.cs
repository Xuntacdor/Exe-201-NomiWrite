namespace NomiWrite.Shared.Contracts.Events.Auth;

/// <summary>
/// Published by Auth Service when a new user registers.
/// Consumed by User Service (to create profile), Notification Service, etc.
/// </summary>
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
}
