namespace NomiWrite.Shared.Contracts.Events.Forum;

public sealed record PostLikedEvent(
    Guid PostId,
    Guid PostAuthorUserId,
    Guid LikerUserId);
