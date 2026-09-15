using FluentValidation;
using NomiWrite.Admin.Application.DTOs;

namespace NomiWrite.Admin.Application.Validation;

public class AnnouncementRequestValidator : AbstractValidator<AnnouncementRequestDto>
{
    public AnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(5000).WithMessage("Message must not exceed 5000 characters.");
    }
}
