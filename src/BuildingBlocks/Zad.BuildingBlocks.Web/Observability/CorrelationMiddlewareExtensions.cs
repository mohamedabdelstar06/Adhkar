using Microsoft.AspNetCore.Builder;

namespace Zad.BuildingBlocks.Web.Observability;

public static class CorrelationMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationMiddleware>();
}

