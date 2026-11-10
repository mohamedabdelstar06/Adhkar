using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Migrations;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Contracts.Routing;

namespace Zad.BuildingBlocks.Web.Tenancy;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantDirectory directory,
        ITenantContextSetter setter,
        ITenantDatabaseReadiness readiness,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<TenantResolutionMiddleware> logger)
    {
        if (TenantRouteConvention.IsTenantless(context.Request.Path))
        {
            await next(context);
            return;
        }

        var host = HostNormalization.Normalize(context.Request.Host.Host);
        var rootDomain = configuration["Tenancy:RootDomain"];
        var isPlatformApi = context.Request.Path.StartsWithSegments("/api/platform", StringComparison.OrdinalIgnoreCase);
        var isApi = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

        if (isPlatformApi)
        {
            await next(context);
            return;
        }

        if (PlatformHost.IsPlatformHost(host, rootDomain) && !isApi)
        {
            await next(context);
            return;
        }

        var slug = HostNormalization.SingleLabelUnder(host, rootDomain);

        var tenant = slug is null || ReservedSlugs.Contains(slug)
            ? null
            : await directory.ResolveByHostAsync(host!, context.RequestAborted)
              ?? await directory.ResolveBySlugAsync(slug, context.RequestAborted);

        if (tenant is null)
        {
            if (!isApi)
            {
                await next(context);
                return;
            }

            logger.LogWarning("No tenant resolved for the request host");

            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                TenancyErrorCodes.TenantNotFound,
                "The host does not belong to a known tenant");

            return;
        }

        if (!tenant.IsActive)
        {
            logger.LogWarning("Tenant {TenantId} is not active (status: {Status})", tenant.TenantId, tenant.Status);

            if (!isApi)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("<!DOCTYPE html><html dir=\"rtl\" lang=\"ar\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>Ø§Ù„Ø®Ø¯Ù…Ø© Ù…Ø¹Ø·Ù„Ø©</title><style>body{font-family:system-ui,-apple-system,sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;background:#f8fafc;color:#1e293b}div{text-align:center;padding:2.5rem;background:#fff;border-radius:16px;box-shadow:0 10px 25px -5px rgba(0,0,0,0.08);max-width:420px;border:1px solid #e2e8f0}.icon{width:64px;height:64px;border-radius:50%;background:#fee2e2;color:#dc2626;display:flex;align-items:center;justify-content:center;font-size:28px;margin:0 auto 1.25rem}h1{color:#0f172a;font-size:1.5rem;margin:0 0 0.5rem;font-weight:700}p{color:#64748b;font-size:0.95rem;line-height:1.6;margin:0}</style></head><body><div><div class=\"icon\">âœ•</div><h1>ØªÙ… Ø¥ÙŠÙ‚Ø§Ù Ø§Ù„Ø®Ø¯Ù…Ø©</h1><p>Ù‡Ø°Ø§ Ø§Ù„Ù…Ø³ØªØ£Ø¬Ø± Ù…Ø¹Ø·Ù„ Ø­Ø§Ù„ÙŠØ§Ù‹ Ù…Ù† Ù‚ÙØ¨Ù„ Ø¥Ø¯Ø§Ø±Ø© Ø§Ù„Ù…Ù†ØµØ© ÙˆÙ„Ø§ ÙŠØ³ØªÙ‚Ø¨Ù„ Ø£ÙŠ Ø²ÙŠØ§Ø±Ø§Øª.</p></div></body></html>");
                return;
            }

            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                TenancyErrorCodes.TenantSuspended,
                "This tenant is suspended and not allowed to serve traffic");

            return;
        }

        setter.Set(tenant.TenantId);
        context.Features.Set<ITenantFeature>(new TenantFeature(tenant));

        if (tenant.Placement == TenantDatabasePlacement.Dedicated &&
            !await readiness.IsReadyAsync(tenant.DatabaseId, context.RequestAborted))
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                TenancyErrorCodes.TenantDatabaseUpdating,
                "This tenant's database is being updated");

            return;
        }

        using (logger.BeginScope(new Dictionary<string, object> { [LogFields.TenantId] = tenant.TenantId }))
        {
            await next(context);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string code, string title)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://errors.zad.example/{code}",
            title,
            status = statusCode,
            code
        });
    }
}
