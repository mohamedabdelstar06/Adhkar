using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Web.Localization;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.BuildingBlocks.UnitTests.Web;

public sealed class CultureResolverTests
{
    private static TenantInfo CreateTenant(string defaultLanguage, params string[] enabledLanguages) =>
        new(
            GuidV7.New(),
            "alpha",
            "alpha.localhost",
            "Active",
            GuidV7.New(),
            TenantDatabasePlacement.Shared,
            defaultLanguage,
            enabledLanguages,
            "UTC",
            true);

    [Fact]
    public void Resolve_Returns_Highest_Quality_Enabled_Language()
    {
        var tenant = CreateTenant("ar", "ar", "en", "fr");

        var resolved = CultureResolver.Resolve(tenant, "de-DE, fr-FR;q=0.9, ar;q=0.8");

        resolved.Should().Be("fr");
    }

    [Fact]
    public void Resolve_Falls_Back_To_Tenant_Default_When_Header_Is_Missing_Or_Unsupported()
    {
        var tenant = CreateTenant("ar", "ar", "en");

        CultureResolver.Resolve(tenant, null).Should().Be("ar");
        CultureResolver.Resolve(tenant, "es-ES, de;q=0.9").Should().Be("ar");
    }

    [Fact]
    public void Resolve_Falls_Back_To_English_When_Tenant_Default_Is_Not_Enabled()
    {
        var tenant = CreateTenant("es", "fr");

        CultureResolver.Resolve(tenant, "de").Should().Be("en");
    }

    [Fact]
    public void PrimaryTags_Orders_By_Quality_And_Ignores_Wildcard()
    {
        var tags = CultureResolver.PrimaryTags("*, en-US;q=0.4, ar-EG;q=0.9, fr;q=0.9");

        tags.Should().Equal("ar", "fr", "en");
    }
}
