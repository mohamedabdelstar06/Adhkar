using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Domain.Localization;

namespace Zad.BuildingBlocks.UnitTests.Domain;

public sealed class LocalizedTextTests
{
    [Fact]
    public void Create_Fails_When_English_Is_Missing_Or_Blank()
    {
        var missing = LocalizedText.Create(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ar"] = "مرحبا"
        });

        missing.IsFailure.Should().BeTrue();
        missing.Error.Should().Be(LocalizedTextErrors.EnglishRequired);

        var blank = LocalizedText.Create(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["en"] = "   ",
            ["ar"] = "مرحبا"
        });

        blank.IsFailure.Should().BeTrue();
        blank.Error.Should().Be(LocalizedTextErrors.EnglishRequired);
    }

    [Fact]
    public void Create_Normalizes_Keys_And_Trims_Values()
    {
        var result = LocalizedText.Create(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [" EN "] = "  Hello  ",
            ["AR"] = "  مرحبا  ",
            ["fr"] = "   "
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Translations.Should().ContainKeys("en", "ar");
        result.Value.Translations.Should().NotContainKey("fr");
        result.Value.Resolve("EN").Should().Be("Hello");
        result.Value.Resolve(" ar ").Should().Be("مرحبا");
    }

    [Fact]
    public void Resolve_Falls_Back_To_English_When_Requested_Language_Is_Absent()
    {
        var text = LocalizedText.Create(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["en"] = "Fallback",
            ["ar"] = "عربي"
        }).Value;

        text.Resolve("fr").Should().Be("Fallback");
        text.Resolve(null!).Should().Be("Fallback");
    }
}
