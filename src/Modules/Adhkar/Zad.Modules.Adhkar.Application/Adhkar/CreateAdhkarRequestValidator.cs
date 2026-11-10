using FluentValidation;

namespace Zad.Modules.Adhkar.Application.Adhkar;

public sealed class CreateAdhkarRequestValidator : AbstractValidator<CreateAdhkarRequest>
{
    public CreateAdhkarRequestValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty().WithMessage("The dhikr text is required")
            .MaximumLength(1000).WithMessage("The dhikr text must not exceed 1000 characters");

        RuleFor(request => request.Translation)
            .MaximumLength(2000).WithMessage("The translation must not exceed 2000 characters");

        RuleFor(request => request.Reference)
            .MaximumLength(300).WithMessage("The reference must not exceed 300 characters");

        RuleFor(request => request.Category)
            .NotEmpty().WithMessage("The category is required");

        RuleFor(request => request.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative");
    }
}

