using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.Domain.Databases;

public static class TenantDatabaseErrors
{
    public static readonly Error NotFound =
        Error.NotFound(TenancyErrorCodes.DatabaseNotFound, "The requested database is not registered");

    public static readonly Error Duplicate =
        Error.Conflict(TenancyErrorCodes.DuplicateDatabase, "A database with this name is already registered");

    public static readonly Error InvalidName =
        Error.Validation(TenancyErrorCodes.DuplicateDatabase, "A database must have a name");
}
