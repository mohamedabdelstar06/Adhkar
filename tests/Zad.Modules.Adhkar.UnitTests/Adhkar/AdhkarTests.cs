using AwesomeAssertions;
using Xunit;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Domain.Adhkar;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;

namespace Zad.Modules.Adhkar.UnitTests.Adhkar;

public sealed class AdhkarTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Rejects_Blank_Text(string text)
    {
        var result = AdhkarEntity.Create(text, "Glory", "Muslim", AdhkarCategories.Tasbih, 1, true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdhkarErrors.TextRequired);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    public void Create_Rejects_Unsupported_Category(string category)
    {
        var result = AdhkarEntity.Create("سبحان الله", "Glory", "Muslim", category, 1, true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AdhkarErrors.InvalidCategory);
    }

    [Theory]
    [InlineData("TASBIH", true)]
    [InlineData("  hawqala ", true)]
    [InlineData("other", false)]
    public void Categories_IsValid_Normalizes_Casing_And_Whitespace(string category, bool expected)
    {
        AdhkarCategories.IsValid(category).Should().Be(expected);
    }

    [Fact]
    public void Create_And_Update_Normalize_Category_And_Fields()
    {
        var created = AdhkarEntity.Create("  سبحان الله  ", "Glory", "Muslim", " TASBIH ", 1, true);
        created.IsSuccess.Should().BeTrue();
        var dhikr = created.Value;
        dhikr.Category.Should().Be(AdhkarCategories.Tasbih);

        var updated = dhikr.Update("الحمد لله", "Praise", "Muslim 2694", "Tahmid", 2, false);
        updated.IsSuccess.Should().BeTrue();
        dhikr.Text.Should().Be("الحمد لله");
        dhikr.Category.Should().Be(AdhkarCategories.Tahmid);
        dhikr.DisplayOrder.Should().Be(2);
        dhikr.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CreateValidator_Rejects_Missing_Text_Category_And_Negative_Order()
    {
        var validator = new CreateAdhkarRequestValidator();

        var invalid = validator.Validate(new CreateAdhkarRequest("", "", "", "", -1, true));

        invalid.IsValid.Should().BeFalse();
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAdhkarRequest.Text));
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAdhkarRequest.Category));
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAdhkarRequest.DisplayOrder));
    }
}

