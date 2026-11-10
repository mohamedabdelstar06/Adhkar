using Microsoft.AspNetCore.DataProtection;
using Zad.BuildingBlocks.Persistence.DataProtection;
using Zad.Modules.Tenancy.Application.Security;

namespace Zad.Modules.Tenancy.Infrastructure.Security;

public sealed class DataProtectionConnectionStringProtector : IConnectionStringProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionConnectionStringProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(DataProtectionServiceCollectionExtensions.TenantDatabasesPurpose);
    }

    public string Protect(string plainText) => _protector.Protect(plainText);

    public string Unprotect(string protectedValue)
    {
        try
        {
            return _protector.Unprotect(protectedValue);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return protectedValue;
        }
    }
}
