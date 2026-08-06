using SufiChain.SufiPlatform.Editions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Editions;

public class EditionsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(EditionsPermissions.GroupName, L("Permission:Editions"));

        var editions = group.AddPermission(
            EditionsPermissions.Editions.Default,
            L("Permission:Editions.Editions"),
            multiTenancySide: MultiTenancySides.Host);

        editions.AddChild(EditionsPermissions.Editions.Create, L("Permission:Editions.Editions.Create"));
        editions.AddChild(EditionsPermissions.Editions.Update, L("Permission:Editions.Editions.Update"));
        editions.AddChild(EditionsPermissions.Editions.Delete, L("Permission:Editions.Editions.Delete"));
        editions.AddChild(EditionsPermissions.Editions.ManageFeatures, L("Permission:Editions.Editions.ManageFeatures"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EditionsResource>(name);
    }
}
