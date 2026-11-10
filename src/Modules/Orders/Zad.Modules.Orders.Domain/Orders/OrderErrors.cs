using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.Modules.Orders.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error NotFound =
        Error.NotFound("orders.order-not-found", "The requested order was not found");

    public static readonly Error CannotCancel =
        Error.Conflict("orders.cannot-cancel", "Only orders in Placed status can be cancelled");

    public static readonly Error EmptyOrder =
        Error.Validation("orders.empty-order", "An order must contain at least one item");

    public static readonly Error ProductNotFound =
        Error.NotFound("orders.product-not-found", "A product in this order does not exist in the catalog");

    public static readonly Error InsufficientStock =
        Error.Unprocessable("orders.insufficient-stock", "A product does not have enough stock for this order");

    public static readonly Error InvalidStatus =
        Error.Conflict("orders.invalid-status", "The order is not in a valid status for this transition");
}

