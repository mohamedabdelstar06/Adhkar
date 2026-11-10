using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Orders.Application.Orders;

namespace Zad.Modules.Orders.Api.Orders;

internal sealed class CreateOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapOrdersGroup()
            .MapPost(string.Empty, HandleAsync)
            .WithValidation<CreateOrderRequest>()
            .WithName("CreateOrder");
    }

    private static async Task<IResult> HandleAsync(
        CreateOrderRequest request,
        OrderPlacementService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateOrderAsync(request, cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedResult($"/api/orders/{result.Value.Id}")
            : result.ToHttpResult();
    }
}
