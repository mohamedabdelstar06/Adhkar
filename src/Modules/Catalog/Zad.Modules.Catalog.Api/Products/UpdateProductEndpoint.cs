using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Catalog.Application.Products;

namespace Zad.Modules.Catalog.Api.Products;

internal sealed class UpdateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapCatalogGroup()
            .MapPut("/products/{id:guid}", HandleAsync)
            .WithValidation<UpdateProductRequest>()
            .WithName("UpdateProduct");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateProductRequest request,
        ProductLifecycleService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
