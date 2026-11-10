using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Zad.Modules.Orders.Api.Orders;

public static class OrdersEndpoints
{
    public static RouteGroupBuilder MapOrdersGroup(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api/orders");
}
