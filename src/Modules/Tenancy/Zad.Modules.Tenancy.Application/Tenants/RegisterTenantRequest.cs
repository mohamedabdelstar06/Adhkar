namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed record RegisterTenantRequest(
    string Slug,
    string Host,
    Guid? DatabaseId,
    string DefaultLanguage,
    string TimeZone);

