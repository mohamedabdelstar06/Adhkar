using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Orders.Domain.Orders;

public readonly record struct OrderId(Guid Value) : IEntityId<OrderId>
{
    public static OrderId New() => new(GuidV7.New());

    public static OrderId From(Guid value) => new(value);

    public static OrderId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out OrderId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new OrderId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}

