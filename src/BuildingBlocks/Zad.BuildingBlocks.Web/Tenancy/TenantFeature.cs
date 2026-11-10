using Zad.Modules.Tenancy.Contracts;

namespace Zad.BuildingBlocks.Web.Tenancy;

public sealed record TenantFeature(TenantInfo Tenant) : ITenantFeature;

