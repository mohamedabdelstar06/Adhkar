using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Catalog.Domain.Products;

public readonly record struct ProductId(Guid Value) : IEntityId<ProductId>
{
    public static ProductId New() => new(GuidV7.New());

    public static ProductId From(Guid value) => new(value);

    public static ProductId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ProductId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new ProductId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
