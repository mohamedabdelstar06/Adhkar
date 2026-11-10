namespace Zad.Modules.Tenancy.Application.Security;

public interface IConnectionStringProtector
{
    public string Protect(string plainText);

    public string Unprotect(string protectedValue);
}
