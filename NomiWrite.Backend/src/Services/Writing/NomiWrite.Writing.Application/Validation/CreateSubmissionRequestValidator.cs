using FluentValidation;
using NomiWrite.Writing.Application.DTOs;

namespace NomiWrite.Writing.Application.Validation;

public class CreateSubmissionRequestValidator : AbstractValidator<CreateSubmissionRequestDto>
{
    public CreateSubmissionRequestValidator()
    {
        RuleFor(x => x.WritingPromptId)
            .NotEmpty().WithMessage("WritingPromptId is required.");
    }
}