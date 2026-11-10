using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Catalog.Application.Products;

namespace Zad.Modules.Catalog.Api.Products;

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapCatalogGroup()
            .MapPost("/products", HandleAsync)
            .WithValidation<CreateProductRequest>()
            .WithName("CreateProduct");
    }

    private static async Task<IResult> HandleAsync(
        CreateProductRequest request,
        ProductLifecycleService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedResult($"/api/catalog/products/{result.Value.Id}")
            : result.ToHttpResult();
    }
}
