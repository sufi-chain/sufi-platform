using SufiChain.SufiPlatform.AuditLogging.Permissions;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.AuditLogging.Data;

/// <summary>
/// Grants <see cref="AuditLoggingPermissions.AuditLogs.ViewAll"/> to every role and user
/// that already holds audit-log or entity-change permission in the tenant being seeded.
/// A setting marks the host and each tenant when that backfill has run. Later DbMigrator
/// runs leave the grants alone, including a ViewAll grant an administrator removed.
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
    private readonly ISettingManager _settingManager;

    public AuditLogViewAllGrantDataSeedContributor(
        IPermissionGrantRepository permissionGrantRepository,
        IPermissionManager permissionManager,
        ISettingManager settingManager)
    {
        _permissionGrantRepository = permissionGrantRepository;
        _permissionManager = permissionManager;
        _settingManager = settingManager;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        // fallback: false so a completed host marker does not skip a tenant that has not run yet.
        var marker = await _settingManager.GetOrNullAsync(
            AuditLogViewAllBackfill.CompletedSettingName,
            context.TenantId,
            fallback: false);

        List<PermissionGrant> grants;
        using (_permissionGrantRepository.DisableTracking())
        {
            grants = await _permissionGrantRepository.GetListAsync();
        }

        var plan = AuditLogViewAllBackfill.Plan(
            marker,
            context.TenantId,
            grants.Select(grant => new AuditPermissionGrant(
                grant.TenantId,
                grant.ProviderName,
                grant.ProviderKey,
                grant.Name)),
            AuditLoggingPermissions.AuditLogs.ViewAll,
            QualifyingPermissions,
            ProviderNames);

        if (plan.AlreadyCompleted)
        {
            return;
        }

        foreach (var holder in plan.Holders)
        {
            await _permissionManager.SetAsync(
                AuditLoggingPermissions.AuditLogs.ViewAll,
                holder.ProviderName,
                holder.ProviderKey,
                true);
        }

        await _settingManager.SetForTenantOrGlobalAsync(
            context.TenantId,
            AuditLogViewAllBackfill.CompletedSettingName,
            AuditLogViewAllBackfill.CompletedValue);
    }
}
