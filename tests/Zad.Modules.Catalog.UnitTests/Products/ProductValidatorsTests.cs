using AwesomeAssertions;
using Xunit;
using Zad.Modules.Catalog.Application.Products;

namespace Zad.Modules.Catalog.UnitTests.Products;

public sealed class ProductValidatorsTests
{
    private readonly CreateProductRequestValidator _createValidator = new();
    private readonly UpdateProductRequestValidator _updateValidator = new();

    [Fact]
    public void CreateValidator_Passes_For_Valid_Request()
    {
        var request = new CreateProductRequest("SKU-100", "Desk Lamp", "LED", 120m, 0);

        var result = _createValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateValidator_Rejects_Invalid_Fields()
    {
        var request = new CreateProductRequest(
            new string('X', 51),
            "",
            "Desc",
            0m,
            -1);

        var result = _createValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Sku));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.UnitPrice));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.StockQuantity));
    }

    [Fact]
    public void UpdateValidator_Rejects_Empty_Name_And_Non_Positive_Price()
    {
        var request = new UpdateProductRequest("", "Desc", -5m);

        var result = _updateValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductRequest.Name));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductRequest.UnitPrice));
    }
}

