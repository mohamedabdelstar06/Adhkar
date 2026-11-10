using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Api.Products;

internal sealed class GetProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapCatalogGroup()
            .MapGet("/products/{id:guid}", HandleAsync)
            .WithName("GetProduct");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IProductReader reader,
        CancellationToken cancellationToken)
    {
        var product = await reader.GetByIdAsync(id, cancellationToken);
        return product is null ? ProductErrors.NotFound.ToProblem() : TypedResults.Ok(product);
    }
}
