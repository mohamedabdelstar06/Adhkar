using System.Diagnostics.CodeAnalysis;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.Modules.Tenancy.Domain.Tenants;

public readonly record struct TenantId(Guid Value) : IEntityId<TenantId>
{
    public static TenantId New() => new(GuidV7.New());

    public static TenantId From(Guid value) => new(value);

    public static TenantId Parse(string s, IFormatProvider? provider) => new(Guid.Parse(s, provider));

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out TenantId result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new TenantId(guid);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => Value.ToString();
}
