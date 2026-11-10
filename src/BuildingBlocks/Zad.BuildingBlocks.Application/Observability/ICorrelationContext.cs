namespace Zad.BuildingBlocks.Application.Observability;

public interface ICorrelationContext
{
    public string CorrelationId { get; }
    public Guid? CausationId { get; }
}
