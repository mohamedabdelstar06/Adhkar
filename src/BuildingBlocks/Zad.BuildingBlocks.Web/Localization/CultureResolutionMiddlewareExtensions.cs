using Microsoft.AspNetCore.Builder;

namespace Zad.BuildingBlocks.Web.Localization;

public static class CultureResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseCultureResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<CultureResolutionMiddleware>();
}

