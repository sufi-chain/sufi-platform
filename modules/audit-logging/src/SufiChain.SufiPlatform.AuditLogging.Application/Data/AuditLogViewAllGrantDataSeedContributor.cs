using SufiChain.SufiPlatform.AuditLogging.Permissions;
using SufiChain.SufiPlatform.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.AuditLogging.Data;

/// <summary>
/// Grants <see cref="AuditLoggingPermissions.AuditLogs.ViewAll"/> to every role and user
/// that already holds audit-log or entity-change permission in the tenant being seeded.
/// The DbMigrator runs this once per host and once per tenant, so existing viewers keep
/// every row after ViewAll is introduced.
/// </summary>
public class AuditLogViewAllGrantDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly string[] QualifyingPermissions =
    [
        AuditLoggingPermissions.AuditLogs.Default,
        AuditLoggingPermissions.EntityChanges.Default
    ];

    private static readonly string[] ProviderNames =
    [
        RolePermissionValueProvider.ProviderName,
        UserPermissionValueProvider.ProviderName
    ];

    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IPermissionManager _permissionManager;

    public AuditLogViewAllGrantDataSeedContributor(
        IPermissionGrantRepository permissionGrantRepository,
        IPermissionManager permissionManager)
    {
        _permissionGrantRepository = permissionGrantRepository;
        _permissionManager = permissionManager;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        List<PermissionGrant> grants;
        using (_permissionGrantRepository.DisableTracking())
        {
            grants = await _permissionGrantRepository.GetListAsync();
        }

        var missing = AuditLogViewAllGrantPlanner.HoldersMissingViewAll(
            context.TenantId,
            grants.Select(grant => new AuditPermissionGrant(
                grant.TenantId,
                grant.ProviderName,
                grant.ProviderKey,
                grant.Name)),
            AuditLoggingPermissions.AuditLogs.ViewAll,
            QualifyingPermissions,
            ProviderNames);

        foreach (var holder in missing)
        {
            await _permissionManager.SetAsync(
                AuditLoggingPermissions.AuditLogs.ViewAll,
                holder.ProviderName,
                holder.ProviderKey,
                true);
        }
    }
}
