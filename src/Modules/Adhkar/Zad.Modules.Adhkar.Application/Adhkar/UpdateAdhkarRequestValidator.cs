using FluentValidation;

namespace Zad.Modules.Adhkar.Application.Adhkar;

public sealed class UpdateAdhkarRequestValidator : AbstractValidator<UpdateAdhkarRequest>
{
    public UpdateAdhkarRequestValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty().WithMessage("The dhikr text is required")
            .MaximumLength(1000).WithMessage("The dhikr text must not exceed 1000 characters");

        RuleFor(request => request.Category)
            .NotEmpty().WithMessage("The category is required");

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");
    }
}

