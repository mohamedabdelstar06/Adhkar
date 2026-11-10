using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zad.Modules.Catalog.Api.Products;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api/catalog");
}
