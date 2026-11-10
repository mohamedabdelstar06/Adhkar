using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Orders.Application.Orders;

namespace Zad.Modules.Orders.Api.Orders;

internal sealed class CancelOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapOrdersGroup()
            .MapPost("/{id:guid}/cancel", HandleAsync)
            .WithName("CancelOrder");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        OrderPlacementService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CancelOrderAsync(id, cancellationToken);
        return result.ToHttpResult();
    }
}
