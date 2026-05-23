using SufiChain.SufiAbp.LocalizationManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.LocalizationManagement.Permissions;

public class LocalizationManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var localizationGroup = context.AddGroup(
            LocalizationManagementPermissions.GroupName,
            L("Permission:LocalizationManagement"));

        var textsPermission = localizationGroup.AddPermission(
            LocalizationManagementPermissions.Texts.Default,
            L("Permission:LocalizationTexts"));
        textsPermission.AddChild(
            LocalizationManagementPermissions.Texts.Create,
            L("Permission:LocalizationTexts.Create"));
        textsPermission.AddChild(
            LocalizationManagementPermissions.Texts.Update,
            L("Permission:LocalizationTexts.Update"));
        textsPermission.AddChild(
            LocalizationManagementPermissions.Texts.Delete,
            L("Permission:LocalizationTexts.Delete"));
        textsPermission.AddChild(
            LocalizationManagementPermissions.Texts.Import,
            L("Permission:LocalizationTexts.Import"));
        textsPermission.AddChild(
            LocalizationManagementPermissions.Texts.Export,
            L("Permission:LocalizationTexts.Export"));

        var resourcesPermission = localizationGroup.AddPermission(
            LocalizationManagementPermissions.Resources.Default,
            L("Permission:LocalizationResources"));
        resourcesPermission.AddChild(
            LocalizationManagementPermissions.Resources.Create,
            L("Permission:LocalizationResources"));
        resourcesPermission.AddChild(
            LocalizationManagementPermissions.Resources.Update,
            L("Permission:LocalizationResources"));
        resourcesPermission.AddChild(
            LocalizationManagementPermissions.Resources.Delete,
            L("Permission:LocalizationResources"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpLocalizationManagementResource>(name);
    }
}
