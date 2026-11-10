using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Web.Observability;

public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationContextSetter setter,
        ILogger<CorrelationMiddleware> logger)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var header) &&
                            !string.IsNullOrWhiteSpace(header)
            ? header.ToString()
            : GuidV7.New().ToString("N");

        setter.Set(correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { [LogFields.CorrelationId] = correlationId }))
        {
            await next(context);
        }
    }
}
