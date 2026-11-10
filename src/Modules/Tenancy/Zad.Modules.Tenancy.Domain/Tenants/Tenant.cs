using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Entities;
using Zad.Modules.Tenancy.Contracts.Routing;

namespace Zad.Modules.Tenancy.Domain.Tenants;

public sealed class Tenant : AggregateRoot<TenantId>
{
    private Tenant()
    {
    }

    public string Slug { get; private set; } = default!;
    public string Host { get; private set; } = default!;
    public TenantStatus Status { get; private set; }
    public Guid DatabaseId { get; private set; }
    public string DefaultLanguage { get; private set; } = "en";
    public string EnabledLanguages { get; private set; } = "en";
    public string TimeZone { get; private set; } = "UTC";

    public static Result<Tenant> Register(
        string slug,
        string host,
        Guid databaseId,
        string defaultLanguage,
        string enabledLanguages,
        string timeZone,
        DateTimeOffset now)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        if (normalizedSlug.Length is < 2 or > 63 ||
            !normalizedSlug.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            return Result<Tenant>.Failure(TenantErrors.InvalidSlug);
        }

        if (ReservedSlugs.Contains(normalizedSlug))
        {
            return Result<Tenant>.Failure(TenantErrors.SlugReserved);
        }

        if (databaseId == Guid.Empty)
        {
            return Result<Tenant>.Failure(TenantErrors.DatabaseNotFound);
        }

        _ = now;

        return Result<Tenant>.Success(new Tenant
        {
            Id = TenantId.New(),
            Slug = normalizedSlug,
            Host = host.Trim().ToLowerInvariant(),
            Status = TenantStatus.Provisioning,
            DatabaseId = databaseId,
            DefaultLanguage = string.IsNullOrWhiteSpace(defaultLanguage) ? "en" : defaultLanguage.Trim().ToLowerInvariant(),
            EnabledLanguages = string.IsNullOrWhiteSpace(enabledLanguages) ? "en" : enabledLanguages.Trim().ToLowerInvariant(),
            TimeZone = string.IsNullOrWhiteSpace(timeZone) ? "UTC" : timeZone.Trim()
        });
    }

    public Result Activate()
    {
        if (Status == TenantStatus.Retired)
        {
            return Result.Failure(TenantErrors.Suspended);
        }

        Status = TenantStatus.Active;
        return Result.Success();
    }

    public Result Update(string host, string defaultLanguage, string enabledLanguages, string timeZone)
    {
        var normalizedHost = host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            return Result.Failure(TenantErrors.InvalidHost);
        }

        Host = normalizedHost;
        if (!string.IsNullOrWhiteSpace(defaultLanguage))
        {
            DefaultLanguage = defaultLanguage.Trim().ToLowerInvariant();
        }
        if (!string.IsNullOrWhiteSpace(enabledLanguages))
        {
            EnabledLanguages = enabledLanguages.Trim().ToLowerInvariant();
        }
        if (!string.IsNullOrWhiteSpace(timeZone))
        {
            TimeZone = timeZone.Trim();
        }

        return Result.Success();
    }

    public Result Suspend()
    {
        Status = TenantStatus.Suspended;
        return Result.Success();
    }
}
