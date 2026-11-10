using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Tenancy.Domain.Databases;

public readonly record struct DatabaseId(Guid Value) : IEntityId<DatabaseId>
{
    public static DatabaseId New() => new(GuidV7.New());

    public static DatabaseId From(Guid value) => new(value);

    public static DatabaseId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out DatabaseId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new DatabaseId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}

