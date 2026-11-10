using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.Modules.Catalog.Contracts.Products;

namespace Zad.Modules.Catalog.Api.Products;

internal sealed class ListProductsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapCatalogGroup()
            .MapGet("/products", HandleAsync)
            .WithName("ListProducts");
    }

    private static async Task<IResult> HandleAsync(
        int? page,
        int? pageSize,
        IProductReader reader,
        CancellationToken cancellationToken)
    {
        var result = await reader.ListAsync(
            page.GetValueOrDefault(1),
            pageSize.GetValueOrDefault(10),
            cancellationToken);

        return TypedResults.Ok(result);
    }
}
