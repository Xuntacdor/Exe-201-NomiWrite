namespace NomiWrite.Learning.Domain.Entities;

/// <summary>
/// A single quiz question. Stored inside the <see cref="Quiz.Questions"/> JSON
/// blob. Correct answers and explanations are never returned to the client
/// until an attempt has been submitted.
/// </summary>
public class QuizQuestionItem
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    /// <summary>One of: multiple_choice | fill_blank | rewrite.</summary>
    public string Type { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Sentence { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}