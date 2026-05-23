using SufiChain.SufiAbp.FeatureManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.FeatureManagement;

public class FeaturePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            FeatureManagementPermissions.GroupName,
            L("Permission:FeatureManagement"));

        var features = group.AddPermission(
            FeatureManagementPermissions.Features.Default,
            L("Permission:FeatureManagement.Features"));

        features.AddChild(
            FeatureManagementPermissions.Features.ManageHostFeatures,
            L("Permission:FeatureManagement.ManageHostFeatures"),
            multiTenancySide: MultiTenancySides.Host);

        features.AddChild(
            FeatureManagementPermissions.Features.ManageTenantFeatures,
            L("Permission:FeatureManagement.ManageTenantFeatures"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpFeatureManagementResource>(name);
    }
}
