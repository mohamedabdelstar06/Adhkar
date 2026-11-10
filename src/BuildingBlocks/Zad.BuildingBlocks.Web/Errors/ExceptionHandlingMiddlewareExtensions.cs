using Microsoft.AspNetCore.Builder;

namespace Zad.BuildingBlocks.Web.Errors;

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseZadExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}

