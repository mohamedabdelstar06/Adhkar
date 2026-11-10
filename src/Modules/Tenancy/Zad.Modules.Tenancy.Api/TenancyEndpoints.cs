using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zad.Modules.Tenancy.Api;

public static class TenancyEndpoints
{
    public static RouteGroupBuilder MapTenancyGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api/tenancy");
}
