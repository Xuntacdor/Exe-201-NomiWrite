using FluentValidation;
using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Validation;

public class SubmitQuizAttemptRequestValidator : AbstractValidator<SubmitQuizAttemptRequestDto>
{
    public SubmitQuizAttemptRequestValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty()
            .WithMessage("quizId is required.");

        RuleFor(x => x.Answers)
            .NotNull()
            .WithMessage("answers is required.");
    }
}