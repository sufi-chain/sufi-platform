using SufiChain.SufiPlatform.Application.Localization.Resources.SufiDdd;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Caching;

public class CacheManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            CacheManagementPermissions.GroupName,
            L("Permission:CacheManagement"));

        group.AddPermission(
            CacheManagementPermissions.Manage,
            L("Permission:CacheManagement.Manage"),
            multiTenancySide: MultiTenancySides.Host);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiDddApplicationContractsResource>(name);
    }
}
