namespace NomiWrite.Learning.Domain.Entities;

public class QuizAttempt : Common.BaseEntity
{
    public Guid QuizId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>User answers keyed by quiz question id.</summary>
    public Dictionary<string, string> Answers { get; set; } = new();
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}