using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zad.Modules.Tenancy.Api.Platform;

public static class PlatformTenancyEndpoints
{
    public static RouteGroupBuilder MapPlatformTenancyGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api/platform");
}
