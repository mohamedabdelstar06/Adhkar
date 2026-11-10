using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Tenancy.Application;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Contracts.Routing;
using Zad.Modules.Tenancy.Domain.Databases;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.UnitTests.Tenants;

public sealed class TenantProvisioningServiceTests
{
    private readonly InMemoryTenantRepository _tenants = new();
    private readonly InMemoryDatabaseRepository _databases = new();
    private readonly InMemoryTenancyUnitOfWork _unitOfWork = new();
    private readonly CountingCacheRefresher _refresher = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 10, 10, 0, 0, TimeSpan.Zero));
    private readonly TenantProvisioningService _service;

    public TenantProvisioningServiceTests()
    {
        _service = new TenantProvisioningService(_tenants, _databases, _unitOfWork, _refresher, _time);
    }

    [Theory]
    [InlineData("www")]
    [InlineData("admin")]
    [InlineData("api")]
    public void ReservedSlugs_Contains_Platform_Reserved_Names(string slug)
    {
        ReservedSlugs.Contains(slug).Should().BeTrue();
    }

    [Fact]
    public void RegisterTenantValidator_Rejects_Invalid_Slug_And_Host()
    {
        var validator = new RegisterTenantRequestValidator();

        var invalid = validator.Validate(new RegisterTenantRequest("-BadSlug-", "bad host!", null, "en", "UTC"));

        invalid.IsValid.Should().BeFalse();
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterTenantRequest.Slug));
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterTenantRequest.Host));
    }

    [Fact]
    public async Task RegisterAsync_Returns_NoSharedDatabase_When_No_Shared_Database_Is_Online()
    {
        var result = await _service.RegisterAsync(
            new RegisterTenantRequest("alpha", "alpha.localhost", null, "ar", "UTC"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.NoSharedDatabase);
    }

    [Fact]
    public async Task RegisterAsync_Rejects_Reserved_Slug_Even_When_Shared_Database_Exists()
    {
        var ct = TestContext.Current.CancellationToken;
        await RegisterSharedDatabaseAsync(ct);

        var result = await _service.RegisterAsync(
            new RegisterTenantRequest("admin", "admin.localhost", null, "ar", "UTC"),
            ct);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.SlugReserved);
    }

    [Fact]
    public async Task RegisterAsync_Creates_Active_Tenant_On_Shared_Db_And_Refreshes_Cache()
    {
        var ct = TestContext.Current.CancellationToken;
        await RegisterSharedDatabaseAsync(ct);

        var result = await _service.RegisterAsync(
            new RegisterTenantRequest("alpha", "alpha.localhost", null, "ar", "UTC"),
            ct);

        result.IsSuccess.Should().BeTrue();
        var tenant = await _tenants.FindAsync(TenantId.From(result.Value), ct);
        tenant.Should().NotBeNull();
        tenant!.Slug.Should().Be("alpha");
        tenant.Host.Should().Be("alpha.localhost");
        tenant.Status.Should().Be(TenantStatus.Active);
        _refresher.RefreshCount.Should().Be(2);
    }

    [Fact]
    public async Task RegisterAsync_Is_Idempotent_On_Slug_And_Rejects_Duplicate_Host()
    {
        var ct = TestContext.Current.CancellationToken;
        await RegisterSharedDatabaseAsync(ct);

        var first = await _service.RegisterAsync(
            new RegisterTenantRequest("alpha", "alpha.localhost", null, "ar", "UTC"),
            ct);
        var idempotent = await _service.RegisterAsync(
            new RegisterTenantRequest("alpha", "alpha.localhost", null, "ar", "UTC"),
            ct);
        var duplicateHost = await _service.RegisterAsync(
            new RegisterTenantRequest("bravo", "alpha.localhost", null, "en", "UTC"),
            ct);

        first.Value.Should().Be(idempotent.Value);
        duplicateHost.IsFailure.Should().BeTrue();
        duplicateHost.Error.Should().Be(TenantErrors.DuplicateHost);
    }

    [Fact]
    public async Task Lifecycle_Transitions_Suspend_And_Activate_Require_Online_Database()
    {
        var ct = TestContext.Current.CancellationToken;
        var dedicatedDbId = await _service.RegisterDatabaseAsync(
            new RegisterDatabaseRequest(
                "Dedicated01",
                DatabaseKind.Dedicated,
                "Server=localhost;Database=Dedicated01;Integrated Security=True;",
                "Server=localhost;Database=Dedicated01;Integrated Security=True;"),
            ct);

        var tenantResult = await _service.RegisterAsync(
            new RegisterTenantRequest("gamma", "gamma.localhost", dedicatedDbId.Value, "en", "UTC"),
            ct);
        tenantResult.IsSuccess.Should().BeTrue();

        var notOnlineActivate = await _service.ActivateAsync(tenantResult.Value, ct);
        notOnlineActivate.IsFailure.Should().BeTrue();
        notOnlineActivate.Error.Should().Be(TenantErrors.DatabaseNotOnline);

        (await _service.MarkDatabaseOnlineAsync(dedicatedDbId.Value, ct)).IsSuccess.Should().BeTrue();
        (await _service.ActivateAsync(tenantResult.Value, ct)).IsSuccess.Should().BeTrue();
        (await _service.ActivateAsync(tenantResult.Value, ct)).Error.Should().Be(TenantErrors.AlreadyActive);

        var suspended = await _service.SuspendAsync(tenantResult.Value, ct);
        suspended.IsSuccess.Should().BeTrue();
        (await _tenants.FindAsync(TenantId.From(tenantResult.Value), ct))!.Status.Should().Be(TenantStatus.Suspended);
    }

    private Task<Result<Guid>> RegisterSharedDatabaseAsync(CancellationToken ct) =>
        _service.RegisterDatabaseAsync(
            new RegisterDatabaseRequest(
                "Shared01",
                DatabaseKind.Shared,
                "Server=localhost;Database=Shared01;Integrated Security=True;",
                "Server=localhost;Database=Shared01;Integrated Security=True;"),
            ct);

    private sealed class InMemoryTenantRepository : ITenantRepository
    {
        private readonly List<Tenant> _items = [];

        public Task<Tenant?> FindAsync(TenantId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(t => t.Id == id));

        public Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(t => string.Equals(t.Slug, slug, StringComparison.Ordinal)));

        public Task<Tenant?> FindByHostAsync(string host, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(t => string.Equals(t.Host, host, StringComparison.Ordinal)));

        public void Add(Tenant tenant) => _items.Add(tenant);

        public void Remove(Tenant tenant) => _items.Remove(tenant);
    }

    private sealed class InMemoryDatabaseRepository : ITenantDatabaseRepository
    {
        private readonly List<TenantDatabase> _items = [];

        public Task<TenantDatabase?> FindAsync(DatabaseId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(d => d.Id == id));

        public Task<TenantDatabase?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.Ordinal)));

        public Task<TenantDatabase?> FindOnlineSharedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(d => d.Kind == DatabaseKind.Shared && d.Status == DatabaseLifecycle.Online));

        public void Add(TenantDatabase database) => _items.Add(database);
    }

    private sealed class InMemoryTenancyUnitOfWork : ITenancyUnitOfWork
    {
        public Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

        public Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> work, CancellationToken cancellationToken = default) =>
            work(cancellationToken);

        public Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> work, CancellationToken cancellationToken = default) =>
            work(cancellationToken);
    }

    private sealed class CountingCacheRefresher : ITenancyCacheRefresher
    {
        public int RefreshCount { get; private set; }

        public Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            RefreshCount++;
            return Task.CompletedTask;
        }
    }
}
