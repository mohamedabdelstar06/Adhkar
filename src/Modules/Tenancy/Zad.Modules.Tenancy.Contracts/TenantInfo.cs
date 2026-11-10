namespace Zad.Modules.Tenancy.Contracts;

public sealed record TenantInfo(
    Guid TenantId,
    string Slug,
    string Host,
    string Status,
    Guid DatabaseId,
    TenantDatabasePlacement Placement,
    string DefaultLanguage,
    IReadOnlyList<string> EnabledLanguages,
    string TimeZone,
    bool IsActive);
