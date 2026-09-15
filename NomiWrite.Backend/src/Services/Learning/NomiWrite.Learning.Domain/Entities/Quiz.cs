namespace NomiWrite.Learning.Domain.Entities;

public class Quiz : Common.BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? SourceSubmissionId { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<QuizQuestionItem> Questions { get; set; } = new();
}