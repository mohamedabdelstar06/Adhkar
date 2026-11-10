using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.MigrationRunner.Seeding;

public sealed class RunnerCorrelationContext : ICorrelationContext, ICorrelationContextSetter
{
    public string CorrelationId { get; private set; } = GuidV7.New().ToString("N");

    public Guid? CausationId { get; private set; }

    public void Set(string correlationId, Guid? causationId = null)
    {
        CorrelationId = correlationId;
        CausationId = causationId;
    }
}

