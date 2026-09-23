using FluentValidation;
using NomiWrite.Learning.Application.DTOs;

namespace NomiWrite.Learning.Application.Validation;

public class GenerateStudyGuideRequestValidator : AbstractValidator<GenerateStudyGuideRequestDto>
{
    public GenerateStudyGuideRequestValidator()
    {
        RuleFor(x => x.TargetExam)
            .MaximumLength(200)
            .WithMessage("targetExam must be at most 200 characters.");

        RuleFor(x => x.TargetBand)
            .InclusiveBetween(1m, 9m)
            .WithMessage("targetBand must be between 1.0 and 9.0.");
    }
}