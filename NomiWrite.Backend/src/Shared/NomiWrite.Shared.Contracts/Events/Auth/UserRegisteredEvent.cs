namespace NomiWrite.Shared.Contracts.Events.Auth;

public sealed record UserRegisteredEvent(Guid Id, string Email, string FullName);