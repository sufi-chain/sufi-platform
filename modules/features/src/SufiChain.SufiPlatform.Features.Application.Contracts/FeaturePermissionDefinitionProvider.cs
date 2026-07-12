using SufiChain.SufiPlatform.Features.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Features;

public class FeaturePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            FeaturesPermissions.GroupName,
            L("Permission:Features"));

        var features = group.AddPermission(
            FeaturesPermissions.Features.Default,
            L("Permission:Features.Features"));

        features.AddChild(
            FeaturesPermissions.Features.ManageHostFeatures,
            L("Permission:Features.ManageHostFeatures"),
            multiTenancySide: MultiTenancySides.Host);

        features.AddChild(
            FeaturesPermissions.Features.ManageTenantFeatures,
            L("Permission:Features.ManageTenantFeatures"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiFeaturesResource>(name);
    }
}
