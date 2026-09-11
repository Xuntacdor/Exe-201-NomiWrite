namespace NomiWrite.Shared.Contracts.Events.Forum;

public sealed record ForumCommentCreatedEvent(
    Guid PostId,
    Guid CommentId,
    Guid PostAuthorUserId,
    Guid CommenterUserId,
    string CommentPreview);
