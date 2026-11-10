using Microsoft.AspNetCore.Builder;

namespace Zad.BuildingBlocks.Web.Tenancy;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantResolutionMiddleware>();
}

