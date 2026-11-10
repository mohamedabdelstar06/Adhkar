using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Orders.Contracts.Orders;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Api.Orders;

internal sealed class GetOrderEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapOrdersGroup()
            .MapGet("/{id:guid}", HandleAsync)
            .WithName("GetOrder");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IOrderReader reader,
        CancellationToken cancellationToken)
    {
        var order = await reader.GetByIdAsync(id, cancellationToken);
        return order is null ? OrderErrors.NotFound.ToProblem() : TypedResults.Ok(order);
    }
}
