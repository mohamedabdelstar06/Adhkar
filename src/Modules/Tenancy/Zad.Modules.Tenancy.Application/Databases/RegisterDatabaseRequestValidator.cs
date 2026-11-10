using FluentValidation;

namespace Zad.Modules.Tenancy.Application.Databases;

public sealed class RegisterDatabaseRequestValidator : AbstractValidator<RegisterDatabaseRequest>
{
    public RegisterDatabaseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters");

        RuleFor(x => x.Kind)
            .IsInEnum().WithMessage("Kind must be Shared or Dedicated");

        RuleFor(x => x.AppConnectionString)
            .NotEmpty().WithMessage("Application connection string is required");

        RuleFor(x => x.MigratorConnectionString)
            .NotEmpty().WithMessage("Migrator connection string is required");
    }
}

