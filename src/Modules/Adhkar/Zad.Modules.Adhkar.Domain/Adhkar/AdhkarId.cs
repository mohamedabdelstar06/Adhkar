using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Adhkar.Domain.Adhkar;

public readonly record struct AdhkarId(Guid Value) : IEntityId<AdhkarId>
{
    public static AdhkarId New() => new(GuidV7.New());

    public static AdhkarId From(Guid value) => new(value);

    public static AdhkarId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out AdhkarId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new AdhkarId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}

