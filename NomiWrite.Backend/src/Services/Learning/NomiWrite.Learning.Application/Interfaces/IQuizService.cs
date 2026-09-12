using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Interfaces;

public interface IQuizService
{
    Task<QuizDto> GenerateQuizAsync(Guid userId, GenerateQuizRequestDto request);

    Task<QuizDetailDto> GetQuizAsync(Guid userId, Guid quizId);

    Task<QuizAttemptResultDto> SubmitAttemptAsync(Guid userId, SubmitQuizAttemptRequestDto request);
}