namespace Zad.BuildingBlocks.Domain.Identifiers;

public static class GuidV7
{
    public static Guid New() => Guid.CreateVersion7();
}

