using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Orders.Domain.Orders;

public readonly record struct OrderItemId(Guid Value) : IEntityId<OrderItemId>
{
    public static OrderItemId New() => new(GuidV7.New());

    public static OrderItemId From(Guid value) => new(value);

    public static OrderItemId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out OrderItemId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new OrderItemId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}

