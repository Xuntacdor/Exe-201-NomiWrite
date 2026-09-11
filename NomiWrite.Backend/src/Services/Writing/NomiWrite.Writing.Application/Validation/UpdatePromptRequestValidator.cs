using FluentValidation;
using NomiWrite.Writing.Application.DTOs;

namespace NomiWrite.Writing.Application.Validation;

public class UpdatePromptRequestValidator : AbstractValidator<UpdatePromptRequestDto>
{
    public UpdatePromptRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

        RuleFor(x => x.Instructions)
            .NotEmpty().WithMessage("Instructions are required.")
            .MaximumLength(5000).WithMessage("Instructions must not exceed 5000 characters.");

        RuleFor(x => x.Difficulty)
            .IsInEnum().WithMessage("Difficulty is not valid.");

        RuleFor(x => x.MinWords)
            .GreaterThanOrEqualTo(0).When(x => x.MinWords.HasValue)
            .WithMessage("MinWords must be non-negative.");

        RuleFor(x => x.MaxWords)
            .GreaterThanOrEqualTo(1).When(x => x.MaxWords.HasValue)
            .WithMessage("MaxWords must be at least 1.");

        RuleFor(x => x)
            .Must(x => !x.MinWords.HasValue || !x.MaxWords.HasValue || x.MinWords <= x.MaxWords)
            .WithMessage("MinWords must be less than or equal to MaxWords.");
    }
}