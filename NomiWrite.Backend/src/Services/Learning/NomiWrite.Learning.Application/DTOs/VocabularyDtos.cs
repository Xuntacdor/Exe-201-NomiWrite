namespace NomiWrite.Learning.Application.DTOs;

public class VocabDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SubmissionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string OriginalWord { get; set; } = string.Empty;
    public string SuggestedWord { get; set; } = string.Empty;
    public string ExampleSentence { get; set; } = string.Empty;
    public bool IsMastered { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VocabularyPageDto
{
    public List<VocabDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UpdateMasteredRequestDto
{
    public bool IsMastered { get; set; }
}

public class GrammarErrorDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid UserId { get; set; }
    public string GrammarCategory { get; set; } = string.Empty;
    public string Sentence { get; set; } = string.Empty;
    public string ErrorPart { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}