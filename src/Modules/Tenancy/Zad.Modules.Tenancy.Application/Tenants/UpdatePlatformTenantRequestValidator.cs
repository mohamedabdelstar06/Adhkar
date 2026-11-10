using FluentValidation;

namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed class UpdatePlatformTenantRequestValidator : AbstractValidator<UpdatePlatformTenantRequest>
{
    public UpdatePlatformTenantRequestValidator()
    {
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

