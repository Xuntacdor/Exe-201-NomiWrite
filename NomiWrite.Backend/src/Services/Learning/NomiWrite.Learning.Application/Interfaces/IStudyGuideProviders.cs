using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Interfaces;

/// <summary>
/// Produces a personalized study guide from the user's writing history using a
/// structured JSON prompt to an LLM provider (Gemini).
/// </summary>
public interface IStudyGuideAiProvider
{
    Task<StudyGuideResult> GenerateGuideAsync(StudyGuideGenerationRequest request);
}

/// <summary>
/// Deterministic, offline fallback that still produces a structurally valid
/// study guide when the LLM provider is unavailable.
/// </summary>
public interface IFallbackStudyGuideProvider
{
    StudyGuideResult GenerateGuide(StudyGuideGenerationRequest request);
}