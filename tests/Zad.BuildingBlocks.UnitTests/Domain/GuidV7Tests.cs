using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.UnitTests.Domain;

public sealed class GuidV7Tests
{
    [Fact]
    public void New_Returns_Non_Empty_Version_7_Guids()
    {
        var id = GuidV7.New();

        id.Should().NotBe(Guid.Empty);
        id.Version.Should().Be(7);
    }

    [Fact]
    public async Task Sequential_Guids_Across_Milliseconds_Increase_Lexicographically()
    {
        var first = GuidV7.New();
        await Task.Delay(5, TestContext.Current.CancellationToken);
        var second = GuidV7.New();

        second.Should().NotBe(first);
        second.CompareTo(first).Should().BeGreaterThan(0);
    }
}
