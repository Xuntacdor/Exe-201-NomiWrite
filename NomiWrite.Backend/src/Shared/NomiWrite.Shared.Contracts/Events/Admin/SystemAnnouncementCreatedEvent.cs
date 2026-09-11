namespace NomiWrite.Shared.Contracts.Events.Admin;

public sealed record SystemAnnouncementCreatedEvent(Guid AnnouncementId, string Title, string Message, DateTime CreatedAt);
