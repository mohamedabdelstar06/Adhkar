namespace Zad.Modules.Tenancy.Contracts;

public static class TenancyErrorCodes
{
    public const string TenantNotFound = "tenancy.tenant-not-found";
    public const string TenantSuspended = "tenancy.tenant-suspended";
    public const string TenantDatabaseUpdating = "tenancy.tenant-database-updating";
    public const string DuplicateSlug = "tenancy.duplicate-slug";
    public const string InvalidSlug = "tenancy.invalid-slug";
    public const string SlugReserved = "tenancy.slug-reserved";
    public const string DatabaseNotFound = "tenancy.database-not-found";
    public const string DuplicateDatabase = "tenancy.duplicate-database";
    public const string DuplicateHost = "tenancy.duplicate-host";
    public const string InvalidHost = "tenancy.invalid-host";
    public const string DatabaseNotOnline = "tenancy.database-not-online";
    public const string TenantAlreadyActive = "tenancy.tenant-already-active";
    public const string NoSharedDatabase = "tenancy.no-shared-database";
}
