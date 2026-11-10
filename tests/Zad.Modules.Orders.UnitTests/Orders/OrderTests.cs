using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.UnitTests.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Place_Fails_When_Order_Has_No_Items()
    {
        var order = Order.Create("ORD-001", "buyer@zad.test").Value;

        var result = order.Place();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.EmptyOrder);
    }

    [Fact]
    public void AddItem_And_Place_Computes_Total_And_Sets_Placed_Status()
    {
        var order = Order.Create("ORD-002", "buyer@zad.test").Value;
        order.AddItem(GuidV7.New(), "Product A", 12.50m, 2).IsSuccess.Should().BeTrue();
        order.AddItem(GuidV7.New(), "Product B", 5.00m, 3).IsSuccess.Should().BeTrue();

        var placeResult = order.Place();

        placeResult.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Placed);
        order.TotalAmount.Should().Be(40.00m);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Cancel_Transitions_Placed_Order_And_Rejects_Second_Cancellation()
    {
        var order = Order.Create("ORD-003", "buyer@zad.test").Value;
        order.AddItem(GuidV7.New(), "Item", 10m, 1);
        order.Place();

        var firstCancel = order.Cancel();
        firstCancel.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        var secondCancel = order.Cancel();
        secondCancel.IsFailure.Should().BeTrue();
        secondCancel.Error.Should().Be(OrderErrors.CannotCancel);
    }
}

