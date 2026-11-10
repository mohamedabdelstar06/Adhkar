using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Catalog.Application.Products;

namespace Zad.Modules.Catalog.Api.Products;

internal sealed class DeactivateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapCatalogGroup()
            .MapPost("/products/{id:guid}/deactivate", HandleAsync)
            .WithName("DeactivateProduct");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ProductLifecycleService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeactivateAsync(id, cancellationToken);
        return result.ToHttpResult();
    }
}
