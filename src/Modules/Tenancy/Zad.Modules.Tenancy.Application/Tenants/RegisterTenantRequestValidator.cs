using FluentValidation;

namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed class RegisterTenantRequestValidator : AbstractValidator<RegisterTenantRequest>
{
    public RegisterTenantRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .MaximumLength(63).WithMessage("Slug must not exceed 63 characters")
            .Matches("^[a-z0-9][a-z0-9-]*[a-z0-9]$")
            .WithMessage("Slug must be lowercase letters, digits or hyphens");

        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Host is required")
            .MaximumLength(255).WithMessage("Host must not exceed 255 characters")
            .Matches("^[a-z0-9.-]+$").WithMessage("Host must be a plain host name such as acme.zad.com");

        RuleFor(x => x.DefaultLanguage)
            .MaximumLength(8).WithMessage("Language code must not exceed 8 characters");

        RuleFor(x => x.TimeZone)
            .MaximumLength(64).WithMessage("Time zone must not exceed 64 characters");
    }
}

