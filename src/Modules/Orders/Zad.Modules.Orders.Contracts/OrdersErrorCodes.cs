namespace Zad.Modules.Orders.Contracts;

public static class OrdersErrorCodes
{
    public const string OrderNotFound = "orders.order-not-found";
    public const string CannotCancel = "orders.cannot-cancel";
    public const string EmptyOrder = "orders.empty-order";
    public const string ProductNotFound = "orders.product-not-found";
    public const string InsufficientStock = "orders.insufficient-stock";
    public const string InvalidStatus = "orders.invalid-status";
}
