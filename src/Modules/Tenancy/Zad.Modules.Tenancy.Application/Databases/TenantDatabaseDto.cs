namespace Zad.Modules.Tenancy.Application.Databases;

public sealed record TenantDatabaseDto(
    Guid Id,
    string Kind,
    string Name,
    string Status);
