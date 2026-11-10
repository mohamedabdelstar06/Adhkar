using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Adhkar.Domain.Rotation;

public readonly record struct RotationId(Guid Value) : IEntityId<RotationId>
{
    public static RotationId New() => new(GuidV7.New());

    public static RotationId From(Guid value) => new(value);

    public static RotationId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out RotationId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new RotationId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}

