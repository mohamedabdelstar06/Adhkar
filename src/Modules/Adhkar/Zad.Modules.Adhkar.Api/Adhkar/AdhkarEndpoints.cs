using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zad.Modules.Adhkar.Api.Adhkar;

public static class AdhkarEndpoints
{
    public static RouteGroupBuilder MapAdhkarGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api/adhkar");
}
