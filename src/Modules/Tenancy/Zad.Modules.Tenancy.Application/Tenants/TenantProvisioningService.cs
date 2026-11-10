using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Domain.Databases;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed class TenantProvisioningService(
    ITenantRepository tenants,
    ITenantDatabaseRepository databases,
    ITenancyUnitOfWork unitOfWork,
    ITenancyCacheRefresher registry,
    TimeProvider timeProvider)
{
    public async Task<Result<Guid>> RegisterDatabaseAsync(
        RegisterDatabaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await databases.FindByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
        {
            return Result<Guid>.Success(existing.Id.Value);
        }

        var databaseResult = TenantDatabase.Register(
            request.Kind,
            request.Name,
            request.AppConnectionString,
            request.MigratorConnectionString);

        if (databaseResult.IsFailure)
        {
            return Result<Guid>.Failure(databaseResult.Error);
        }

        var database = databaseResult.Value;

        if (request.Kind == DatabaseKind.Shared)
        {
            database.MarkOnline();
        }

        databases.Add(database);

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return Result<Guid>.Failure(save.Error);
        }

        await registry.RefreshAsync(cancellationToken);

        return Result<Guid>.Success(database.Id.Value);
    }

    public async Task<Result> MarkDatabaseOnlineAsync(Guid databaseId, CancellationToken cancellationToken = default)
    {
        var database = await databases.FindAsync(DatabaseId.From(databaseId), cancellationToken);
        if (database is null)
        {
            return Result.Failure(TenantErrors.DatabaseNotFound);
        }

        database.MarkOnline();

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save;
        }

        await registry.RefreshAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Guid>> RegisterAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await tenants.FindBySlugAsync(request.Slug, cancellationToken);
        if (existing is not null)
        {
            return Result<Guid>.Success(existing.Id.Value);
        }

        var hostTaken = await tenants.FindByHostAsync(request.Host, cancellationToken);
        if (hostTaken is not null)
        {
            return Result<Guid>.Failure(TenantErrors.DuplicateHost);
        }

        var database = request.DatabaseId is { } requestedId && requestedId != Guid.Empty
            ? await databases.FindAsync(DatabaseId.From(requestedId), cancellationToken)
            : await databases.FindOnlineSharedAsync(cancellationToken);

        if (database is null)
        {
            return Result<Guid>.Failure(
                request.DatabaseId is null ? TenantErrors.NoSharedDatabase : TenantErrors.DatabaseNotFound);
        }

        var tenantResult = Tenant.Register(
            request.Slug,
            request.Host,
            database.Id.Value,
            request.DefaultLanguage,
            "ar,en",
            request.TimeZone,
            timeProvider.GetUtcNow());

        if (tenantResult.IsFailure)
        {
            return Result<Guid>.Failure(tenantResult.Error);
        }

        var tenant = tenantResult.Value;

        if (database.Kind == DatabaseKind.Shared)
        {
            tenant.Activate();
        }

        tenants.Add(tenant);

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return Result<Guid>.Failure(save.Error);
        }

        await registry.RefreshAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id.Value);
    }

    public async Task<Result> ActivateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.FindAsync(TenantId.From(tenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        if (tenant.Status == TenantStatus.Active)
        {
            return Result.Failure(TenantErrors.AlreadyActive);
        }

        var database = await databases.FindAsync(DatabaseId.From(tenant.DatabaseId), cancellationToken);
        if (database is null)
        {
            return Result.Failure(TenantErrors.DatabaseNotFound);
        }

        if (database.Status != DatabaseLifecycle.Online)
        {
            return Result.Failure(TenantErrors.DatabaseNotOnline);
        }

        var activate = tenant.Activate();
        if (activate.IsFailure)
        {
            return activate;
        }

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save;
        }

        await registry.RefreshAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SuspendAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.FindAsync(TenantId.From(tenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        var suspend = tenant.Suspend();
        if (suspend.IsFailure)
        {
            return suspend;
        }

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save;
        }

        await registry.RefreshAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(
        Guid tenantId,
        UpdatePlatformTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.FindAsync(TenantId.From(tenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        var normalizedHost = request.Host.Trim().ToLowerInvariant();
        var existingHost = await tenants.FindByHostAsync(normalizedHost, cancellationToken);
        if (existingHost is not null && existingHost.Id != tenant.Id)
        {
            return Result.Failure(TenantErrors.DuplicateHost);
        }

        var updateResult = tenant.Update(
            request.Host,
            request.DefaultLanguage,
            "ar,en",
            request.TimeZone);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save;
        }

        await registry.RefreshAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await tenants.FindAsync(TenantId.From(tenantId), cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound);
        }

        tenants.Remove(tenant);

        var save = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save;
        }

        await registry.RefreshAsync(cancellationToken);

        return Result.Success();
    }
}
