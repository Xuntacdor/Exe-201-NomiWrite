using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Interfaces;

public interface IStudyGuideService
{
    /// <summary>
    /// Returns the latest cached guide for the user without calling the LLM.
    /// Throws <see cref="Exceptions.StudyGuideInsufficientDataException"/> when
    /// the user has no writing history to analyze.
    /// </summary>
    Task<StudyGuideDto?> GetLatestGuideAsync(Guid userId);

    /// <summary>
    /// Analyzes the user's writing history and generates a personalized
    /// improvement roadmap. Persists the result so repeat requests are served
    /// from the stored cache instead of re-running the LLM.
    /// </summary>
    Task<StudyGuideDto> GenerateGuideAsync(Guid userId, GenerateStudyGuideRequestDto request, string? accessToken);
}