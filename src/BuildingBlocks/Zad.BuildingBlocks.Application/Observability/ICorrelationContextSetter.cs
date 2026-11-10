namespace Zad.BuildingBlocks.Application.Observability;

public interface ICorrelationContextSetter
{
    public void Set(string correlationId, Guid? causationId = null);
}

