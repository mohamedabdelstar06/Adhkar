using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Localization;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Web.Tenancy;

namespace Zad.BuildingBlocks.Web.Localization;

public sealed class CultureResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICultureContextSetter setter,
        ILogger<CultureResolutionMiddleware> logger)
    {
        var feature = context.Features.Get<ITenantFeature>();
        var language = feature is null
            ? CultureResolver.DefaultLanguage
            : CultureResolver.Resolve(feature.Tenant, context.Request.Headers.AcceptLanguage.ToString());

        setter.Set(language);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Content-Language"] = language;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { [LogFields.Culture] = language }))
        {
            await next(context);
        }
    }
}
