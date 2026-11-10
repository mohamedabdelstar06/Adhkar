using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Persistence.UnitOfWork;
using Zad.Modules.Adhkar.Application;
using Zad.Modules.Adhkar.Contracts;
using Zad.Modules.Adhkar.Domain.Adhkar;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence;

public sealed class AdhkarUnitOfWork(AdhkarDbContext context)
    : ModuleUnitOfWork<AdhkarDbContext>(context), IAdhkarUnitOfWork
{
    protected override Error ConcurrencyConflict =>
        Error.Conflict(AdhkarErrorCodes.NotFound, "The dhikr was modified concurrently by another process");

    protected override IReadOnlyDictionary<string, Error> UniqueIndexErrors { get; } =
        new Dictionary<string, Error>(StringComparer.OrdinalIgnoreCase)
        {
            ["UX_Adhkar_TenantId_DisplayOrder"] = AdhkarErrors.DuplicateOrder
        };
}
