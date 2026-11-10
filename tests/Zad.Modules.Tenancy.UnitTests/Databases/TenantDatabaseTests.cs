using AwesomeAssertions;
using Xunit;
using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.UnitTests.Databases;

public sealed class TenantDatabaseTests
{
    [Fact]
    public void Register_Rejects_Blank_Name()
    {
        var result = TenantDatabase.Register(
            DatabaseKind.Shared,
            "   ",
            "Server=localhost;Database=Shared;Integrated Security=True;",
            "Server=localhost;Database=Shared;Integrated Security=True;");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantDatabaseErrors.InvalidName);
    }

    [Fact]
    public void Register_Starts_In_Provisioning_And_MarkOnline_Transitions_To_Online()
    {
        var result = TenantDatabase.Register(
            DatabaseKind.Dedicated,
            "Dedicated01",
            "Server=localhost;Database=Dedicated01;Integrated Security=True;",
            "Server=localhost;Database=Dedicated01;Integrated Security=True;");

        result.IsSuccess.Should().BeTrue();
        var database = result.Value;
        database.Kind.Should().Be(DatabaseKind.Dedicated);
        database.Status.Should().Be(DatabaseLifecycle.Provisioning);

        database.MarkOnline();
        database.Status.Should().Be(DatabaseLifecycle.Online);
    }
}

