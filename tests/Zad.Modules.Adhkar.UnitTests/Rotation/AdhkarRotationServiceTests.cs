using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Adhkar.Application;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Application.Rotation;
using Zad.Modules.Adhkar.Domain.Adhkar;
using Zad.Modules.Adhkar.Domain.Rotation;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;

namespace Zad.Modules.Adhkar.UnitTests.Rotation;

public sealed class AdhkarRotationServiceTests
{
    private readonly InMemoryAdhkarRepository _adhkarRepository = new();
    private readonly InMemoryRotationRepository _rotationRepository = new();
    private readonly InMemoryAdhkarUnitOfWork _unitOfWork = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 10, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task CreateAsync_Rejects_Duplicate_DisplayOrder()
    {
        var management = new AdhkarManagementService(_adhkarRepository, _unitOfWork);
        var ct = TestContext.Current.CancellationToken;

        var first = await management.CreateAsync(
            new CreateAdhkarRequest("سبحان الله", "Glory", "Muslim", AdhkarCategories.Tasbih, 1, true),
            ct);
        first.IsSuccess.Should().BeTrue();

        var duplicate = await management.CreateAsync(
            new CreateAdhkarRequest("الحمد لله", "Praise", "Muslim", AdhkarCategories.Tahmid, 1, true),
            ct);
        duplicate.IsFailure.Should().BeTrue();
        duplicate.Error.Should().Be(AdhkarErrors.DuplicateOrder);
    }

    [Fact]
    public async Task RotateAsync_Seeds_Defaults_When_Empty_And_Cycles_In_Order()
    {
        var rotationService = new AdhkarRotationService(_adhkarRepository, _rotationRepository, _unitOfWork, _time);
        var ct = TestContext.Current.CancellationToken;

        var first = await rotationService.RotateAsync(ct);
        first.IsSuccess.Should().BeTrue();
        first.Value.DisplayOrder.Should().Be(1);
        _adhkarRepository.Items.Should().HaveCount(8);

        _time.Advance(TimeSpan.FromMinutes(1));
        var second = await rotationService.RotateAsync(ct);
        second.IsSuccess.Should().BeTrue();
        second.Value.DisplayOrder.Should().Be(2);

        _rotationRepository.State!.RotationCount.Should().Be(2);
        _rotationRepository.State.RotatedAtUtc.Should().Be(_time.GetUtcNow());
    }

    [Fact]
    public async Task RotateAsync_Wraps_Back_To_First_Active_After_Last()
    {
        var a = AdhkarEntity.Create("D1", "T1", "R1", AdhkarCategories.Tasbih, 1, true).Value;
        var b = AdhkarEntity.Create("D2", "T2", "R2", AdhkarCategories.Tahmid, 2, true).Value;
        var inactive = AdhkarEntity.Create("D3", "T3", "R3", AdhkarCategories.Takbir, 3, false).Value;
        _adhkarRepository.Add(b);
        _adhkarRepository.Add(a);
        _adhkarRepository.Add(inactive);

        var rotationService = new AdhkarRotationService(_adhkarRepository, _rotationRepository, _unitOfWork, _time);
        var ct = TestContext.Current.CancellationToken;

        var first = await rotationService.RotateAsync(ct);
        var second = await rotationService.RotateAsync(ct);
        var wrapped = await rotationService.RotateAsync(ct);

        first.Value.Id.Should().Be(a.Id.Value);
        second.Value.Id.Should().Be(b.Id.Value);
        wrapped.Value.Id.Should().Be(a.Id.Value);
    }

    private sealed class InMemoryAdhkarRepository : IAdhkarRepository
    {
        public List<AdhkarEntity> Items { get; } = [];

        public Task<AdhkarEntity?> FindAsync(AdhkarId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(item => item.Id == id));

        public Task<IReadOnlyList<AdhkarEntity>> ListActiveOrderedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AdhkarEntity>>(
                Items.Where(item => item.IsActive).OrderBy(item => item.DisplayOrder).ToList());

        public Task<bool> DisplayOrderTakenAsync(int displayOrder, AdhkarId? excluded, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(item => item.DisplayOrder == displayOrder && item.Id != excluded));

        public void Add(AdhkarEntity adhkar) => Items.Add(adhkar);

        public void Remove(AdhkarEntity adhkar) => Items.Remove(adhkar);
    }

    private sealed class InMemoryRotationRepository : IAdhkarRotationRepository
    {
        public AdhkarRotation? State { get; private set; }

        public Task<AdhkarRotation?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(State);

        public void Add(AdhkarRotation rotation) => State = rotation;
    }

    private sealed class InMemoryAdhkarUnitOfWork : IAdhkarUnitOfWork
    {
        public Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());

        public Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> work, CancellationToken cancellationToken = default) =>
            work(cancellationToken);

        public Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> work, CancellationToken cancellationToken = default) =>
            work(cancellationToken);
    }
}

