using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.Domain.Tenants;

public static class TenantErrors
{
    public static readonly Error NotFound =
        Error.NotFound(TenancyErrorCodes.TenantNotFound, "The requested tenant does not exist");

    public static readonly Error Suspended =
        Error.Forbidden(TenancyErrorCodes.TenantSuspended, "This tenant is not allowed to serve traffic");

    public static readonly Error InvalidSlug =
        Error.Validation(TenancyErrorCodes.InvalidSlug, "A tenant slug must be 2-63 characters of lowercase letters, digits or hyphens");

    public static readonly Error SlugReserved =
        Error.Validation(TenancyErrorCodes.SlugReserved, "This slug is reserved by the platform and cannot be used as a tenant");

    public static readonly Error DuplicateSlug =
        Error.Conflict(TenancyErrorCodes.DuplicateSlug, "A tenant with this slug already exists");

    public static readonly Error DuplicateHost =
        Error.Conflict(TenancyErrorCodes.DuplicateHost, "Another tenant already answers on this host");

    public static readonly Error InvalidHost =
        Error.Validation(TenancyErrorCodes.InvalidHost, "A valid host name is required");

    public static readonly Error DatabaseNotFound =
        Error.NotFound(TenancyErrorCodes.DatabaseNotFound, "The target tenant database is not registered");

    public static readonly Error DatabaseNotOnline =
        Error.Conflict(
            TenancyErrorCodes.DatabaseNotOnline,
            "The target database is still provisioning; run the migration runner and mark it online first");

    public static readonly Error NoSharedDatabase =
        Error.Unprocessable(
            TenancyErrorCodes.NoSharedDatabase,
            "No shared database is registered and online; register one with the migration runner first");

    public static readonly Error AlreadyActive =
        Error.Conflict(TenancyErrorCodes.TenantAlreadyActive, "This tenant is already active");
}
