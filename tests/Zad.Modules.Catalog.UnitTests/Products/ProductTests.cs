using AwesomeAssertions;
using Xunit;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.UnitTests.Products;

public sealed class ProductTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Rejects_Non_Positive_Unit_Price(decimal price)
    {
        var result = Product.Create("SKU-001", "Notebook", "Ruled pages", price, 5);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.InvalidPrice);
    }

    [Fact]
    public void Create_With_Valid_Inputs_Initializes_Active_Product()
    {
        var result = Product.Create("SKU-001", "Notebook", "Ruled pages", 25.50m, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("SKU-001");
        result.Value.Name.Should().Be("Notebook");
        result.Value.Description.Should().Be("Ruled pages");
        result.Value.UnitPrice.Should().Be(25.50m);
        result.Value.StockQuantity.Should().Be(10);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_Rejects_Non_Positive_Price_And_Updates_Valid_Details()
    {
        var product = Product.Create("SKU-002", "Pen", "Blue ink", 5m, 10).Value;

        var invalid = product.UpdateDetails("Gel Pen", "Smooth ink", 0m);
        invalid.IsFailure.Should().BeTrue();
        invalid.Error.Should().Be(ProductErrors.InvalidPrice);

        var updated = product.UpdateDetails("Gel Pen", "Smooth ink", 7.25m);
        updated.IsSuccess.Should().BeTrue();
        product.Name.Should().Be("Gel Pen");
        product.Description.Should().Be("Smooth ink");
        product.UnitPrice.Should().Be(7.25m);
    }

    [Fact]
    public void AdjustStock_Prevents_Negative_Stock_And_Updates_Balance()
    {
        var product = Product.Create("SKU-003", "Book", "Hardcover", 40m, 5).Value;

        var overdraw = product.AdjustStock(-6);
        overdraw.IsFailure.Should().BeTrue();
        overdraw.Error.Should().Be(ProductErrors.InsufficientStock);
        product.StockQuantity.Should().Be(5);

        var deducted = product.AdjustStock(-3);
        deducted.IsSuccess.Should().BeTrue();
        product.StockQuantity.Should().Be(2);

        var restocked = product.AdjustStock(4);
        restocked.IsSuccess.Should().BeTrue();
        product.StockQuantity.Should().Be(6);
    }

    [Fact]
    public void Deactivate_Marks_Product_Inactive()
    {
        var product = Product.Create("SKU-004", "Marker", "Black", 3m, 1).Value;

        var result = product.Deactivate();

        result.IsSuccess.Should().BeTrue();
        product.IsActive.Should().BeFalse();
    }
}

