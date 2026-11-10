namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed record TenantRegistrationDto(
    Guid TenantId,
    string Slug,
    string Host,
    string Status,
    Guid DatabaseId,
    string DefaultLanguage,
    string TimeZone);

