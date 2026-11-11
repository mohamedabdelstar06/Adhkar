using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.UnitTests.Tenants;

public sealed class CurrentTenantReaderTests
{
    [Fact]
    public async Task GetAsync_Returns_Null_When_Tenant_Is_Not_Resolved()
    {
        var context = new StubTenantContext(isResolved: false, Guid.Empty);
        var repository = new StubTenantReadRepository();
        var reader = new CurrentTenantReader(context, repository);

        var result = await reader.GetAsync(TestContext.Current.CancellationToken);

        result.Should().BeNull();
        repository.GetCalls.Should().Be(0);
    }

    [Fact]
    public async Task GetAsync_Returns_Tenant_From_Repository_When_Resolved()
    {
        var tenantId = Guid.CreateVersion7();
        var expected = new TenantInfo(
            tenantId,
            "demo",
            "demo.localhost",
            "Active",
            Guid.CreateVersion7(),
            TenantDatabasePlacement.Shared,
            "ar",
            ["ar", "en"],
            "UTC",
            true);
        var context = new StubTenantContext(isResolved: true, tenantId);
        var repository = new StubTenantReadRepository(expected);
        var reader = new CurrentTenantReader(context, repository);

        var result = await reader.GetAsync(TestContext.Current.CancellationToken);

        result.Should().Be(expected);
        repository.GetCalls.Should().Be(1);
    }

    private sealed class StubTenantContext(bool isResolved, Guid tenantId) : ITenantContext
    {
        public bool IsResolved { get; } = isResolved;

        public Guid TenantId => IsResolved
            ? tenantId
            : throw new InvalidOperationException("Tenant context is not resolved.");
    }

    private sealed class StubTenantReadRepository(TenantInfo? tenant = null) : ITenantReadRepository
    {
        public int GetCalls { get; private set; }

        public Task<TenantInfo?> ResolveByHostAsync(string host, CancellationToken cancellationToken = default) =>
            Task.FromResult<TenantInfo?>(null);

        public Task<TenantInfo?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult<TenantInfo?>(null);

        public Task<TenantInfo?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            GetCalls++;
            return Task.FromResult(tenant is not null && tenant.TenantId == tenantId ? tenant : null);
        }

        public Task<IReadOnlyList<TenantInfo>> ListActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TenantInfo>>([]);

        public Task<IReadOnlyList<TenantRegistrationDto>> ListAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TenantRegistrationDto>>([]);
    }
}
