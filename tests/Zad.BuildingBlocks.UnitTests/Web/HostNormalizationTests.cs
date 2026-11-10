using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Web.Tenancy;

namespace Zad.BuildingBlocks.UnitTests.Web;

public sealed class HostNormalizationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(".")]
    [InlineData("مثال.zad.com")]
    public void Normalize_Returns_Null_For_Blank_Or_Non_Ascii_Hosts(string? host)
    {
        HostNormalization.Normalize(host).Should().BeNull();
    }

    [Theory]
    [InlineData(" Acme.Zad.COM. ", "acme.zad.com")]
    [InlineData("alpha.localhost", "alpha.localhost")]
    public void Normalize_Lowercases_Trims_And_Removes_Trailing_Dot(string host, string expected)
    {
        HostNormalization.Normalize(host).Should().Be(expected);
    }

    [Theory]
    [InlineData("alpha.zad.com", "zad.com", "alpha")]
    [InlineData("ALPHA.ZAD.COM.", "ZAD.COM", "alpha")]
    [InlineData("zad.com", "zad.com", null)]
    [InlineData("a.b.zad.com", "zad.com", null)]
    [InlineData("alpha.other.com", "zad.com", null)]
    public void SingleLabelUnder_Extracts_Single_Subdomain_Label(string? host, string? rootDomain, string? expected)
    {
        HostNormalization.SingleLabelUnder(host, rootDomain).Should().Be(expected);
    }
}
