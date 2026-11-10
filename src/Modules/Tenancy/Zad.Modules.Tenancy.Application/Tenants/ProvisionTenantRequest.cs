namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed record ProvisionTenantRequest(
    string Slug,
    string Host,
    string DefaultLanguage,
    string TimeZone);
