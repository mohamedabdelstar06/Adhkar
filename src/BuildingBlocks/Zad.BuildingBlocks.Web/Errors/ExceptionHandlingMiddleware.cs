using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Observability;

namespace Zad.BuildingBlocks.Web.Errors;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public const string UnexpectedCode = "platform.unexpected";

    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationContext correlation,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request {Path} was aborted by the client", context.Request.Path);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                type = $"https://errors.zad.example/{UnexpectedCode}",
                title = "An unexpected error occurred",
                status = StatusCodes.Status500InternalServerError,
                code = UnexpectedCode,
                traceId = Activity.Current?.Id ?? context.TraceIdentifier,
                correlationId = correlation.CorrelationId
            });
        }
    }
}
