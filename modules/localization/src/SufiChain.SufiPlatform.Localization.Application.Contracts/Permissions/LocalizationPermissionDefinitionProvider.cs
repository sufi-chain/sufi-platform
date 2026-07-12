using SufiChain.SufiPlatform.Localization.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Localization.Permissions;

public class LocalizationPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var localizationGroup = context.AddGroup(
            LocalizationPermissions.GroupName,
            L("Permission:Localization"));

        var textsPermission = localizationGroup.AddPermission(
            LocalizationPermissions.Texts.Default,
            L("Permission:LocalizationTexts"));
        textsPermission.AddChild(
            LocalizationPermissions.Texts.Create,
            L("Permission:LocalizationTexts.Create"));
        textsPermission.AddChild(
            LocalizationPermissions.Texts.Update,
            L("Permission:LocalizationTexts.Update"));
        textsPermission.AddChild(
            LocalizationPermissions.Texts.Delete,
            L("Permission:LocalizationTexts.Delete"));
        textsPermission.AddChild(
            LocalizationPermissions.Texts.Import,
            L("Permission:LocalizationTexts.Import"));
        textsPermission.AddChild(
            LocalizationPermissions.Texts.Export,
            L("Permission:LocalizationTexts.Export"));

        var resourcesPermission = localizationGroup.AddPermission(
            LocalizationPermissions.Resources.Default,
            L("Permission:LocalizationResources"));
        resourcesPermission.AddChild(
            LocalizationPermissions.Resources.Create,
            L("Permission:LocalizationResources"));
        resourcesPermission.AddChild(
            LocalizationPermissions.Resources.Update,
            L("Permission:LocalizationResources"));
        resourcesPermission.AddChild(
            LocalizationPermissions.Resources.Delete,
            L("Permission:LocalizationResources"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiLocalizationResource>(name);
    }
}
