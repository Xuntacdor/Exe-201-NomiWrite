using NomiWrite.Learning.Application.DTOs;
using NomiWrite.Learning.Domain.Entities;

namespace NomiWrite.Learning.Application.Interfaces;

/// <summary>
/// Produces quiz questions from the user's weak points using a structured JSON
/// prompt to an LLM provider (AI Coordinator path).
/// </summary>
public interface IAiQuizProvider
{
    Task<List<QuizQuestionItem>> GenerateQuestionsAsync(QuizGenerationRequest request, int targetCount);
}

/// <summary>
/// Deterministic, offline fallback that still produces useful questions when
/// the LLM provider is unavailable. Keeps quiz generation usable during dev.
/// </summary>
public interface IFallbackQuizProvider
{
    List<QuizQuestionItem> GenerateQuestions(QuizGenerationRequest request, int targetCount);
}