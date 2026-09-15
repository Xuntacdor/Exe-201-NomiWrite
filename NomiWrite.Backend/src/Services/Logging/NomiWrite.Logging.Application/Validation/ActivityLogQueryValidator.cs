using FluentValidation;
using NomiWrite.Logging.Application.DTOs;

namespace NomiWrite.Logging.Application.Validation;

public class ActivityLogQueryValidator : AbstractValidator<ActivityLogQueryDto>
{
    public ActivityLogQueryValidator()
    {
        RuleFor(q => q.PageIndex)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageIndex must be greater than or equal to 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(q => q.FromDate)
            .LessThanOrEqualTo(q => q.ToDate)
            .When(q => q.FromDate.HasValue && q.ToDate.HasValue)
            .WithMessage("FromDate must not be later than ToDate.");

        RuleFor(q => q.UserId)
            .MaximumLength(64)
            .When(q => !string.IsNullOrWhiteSpace(q.UserId))
            .WithMessage("UserId must not exceed 64 characters.");

        RuleFor(q => q.Action)
            .MaximumLength(64)
            .When(q => !string.IsNullOrWhiteSpace(q.Action))
            .WithMessage("Action must not exceed 64 characters.");

        RuleFor(q => q.SortBy)
            .Must(sortBy => sortBy == null ||
                            sortBy == "timestamp" ||
                            sortBy == "action" ||
                            sortBy == "serviceName" ||
                            sortBy == "userId")
            .WithMessage("SortBy must be one of: timestamp, action, serviceName, userId.");
    }
}