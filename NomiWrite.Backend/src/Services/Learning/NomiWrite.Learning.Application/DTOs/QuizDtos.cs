namespace NomiWrite.Learning.Application.DTOs;

public class GenerateQuizRequestDto
{
    /// <summary>Canonical field (see task contract: { "submissionId": "uuid" }).</summary>
    public Guid? SubmissionId { get; set; }

    /// <summary>Frontend alias used by NomiWrite.Frontend (GenerateQuizRequest.sourceSubmissionId).</summary>
    public Guid? SourceSubmissionId { get; set; }

    public List<string>? Categories { get; set; }
    public List<Guid>? VocabularyIds { get; set; }
}

public class QuizQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Sentence { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

/// <summary>
/// Public question shape. Deliberately omits CorrectAnswer and Explanation so
/// answers can never be leaked through the browser Network tab before attempt.
/// </summary>
public class QuizQuestionSafeDto
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Sentence { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

public class QuizDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SourceSubmissionId { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<QuizQuestionSafeDto> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class QuizDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SourceSubmissionId { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<QuizQuestionSafeDto> Questions { get; set; } = new();
    public bool IsCompleted { get; set; }

    /// <summary>Only populated once the caller has completed an attempt.</summary>
    public Dictionary<string, string>? CorrectAnswers { get; set; }

    /// <summary>Only populated once the caller has completed an attempt.</summary>
    public Dictionary<string, string>? Explanations { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class SubmitQuizAttemptRequestDto
{
    public Guid QuizId { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
}

public class QuizQuestionResultDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class QuizAttemptResultDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Guid UserId { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime AttemptedAt { get; set; }
    public List<QuizQuestionResultDto> QuestionBreakdown { get; set; } = new();
}

/// <summary>
/// Neutral source material handed to quiz question providers (AI or local
/// fallback) without leaking persistence concerns.
/// </summary>
public class QuizGenerationRequest
{
    public List<GrammarErrorDto> GrammarErrors { get; set; } = new();
    public List<VocabDto> Vocabulary { get; set; } = new();
    public string? Hint { get; set; }
}