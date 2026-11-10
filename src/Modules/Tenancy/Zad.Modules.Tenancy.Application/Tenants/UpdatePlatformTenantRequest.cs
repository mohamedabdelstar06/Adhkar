namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed record UpdatePlatformTenantRequest(
    string Host,
    string DefaultLanguage,
    string TimeZone);
