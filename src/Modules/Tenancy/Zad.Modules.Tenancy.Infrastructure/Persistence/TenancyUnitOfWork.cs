using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Persistence.UnitOfWork;
using Zad.Modules.Tenancy.Application;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence;

public sealed class TenancyUnitOfWork(TenancyDbContext context)
    : ModuleUnitOfWork<TenancyDbContext>(context), ITenancyUnitOfWork
{
    protected override Error ConcurrencyConflict =>
        Error.Conflict(TenancyErrorCodes.TenantNotFound, "The tenant record was modified concurrently by another process");

    protected override IReadOnlyDictionary<string, Error> UniqueIndexErrors { get; } =
        new Dictionary<string, Error>(StringComparer.OrdinalIgnoreCase)
        {
            ["UX_Tenants_Slug"] = TenantErrors.DuplicateSlug,
            ["UX_Tenants_Host"] = TenantErrors.DuplicateHost
        };
}
