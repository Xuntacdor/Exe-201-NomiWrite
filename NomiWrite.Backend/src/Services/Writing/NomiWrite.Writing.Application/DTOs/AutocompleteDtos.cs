namespace NomiWrite.Writing.Application.DTOs;

public class AutocompleteRequestDto
{
    public Guid? PromptId { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public string? Suffix { get; set; }
}

public class AutocompleteResponseDto
{
    public string Suggestion { get; set; } = string.Empty;
}
