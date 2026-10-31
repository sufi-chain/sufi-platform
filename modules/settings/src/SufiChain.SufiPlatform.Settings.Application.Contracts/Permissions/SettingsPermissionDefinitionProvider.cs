using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.Authorization.Permissions;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Settings;

public class SettingsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            SettingsPermissions.GroupName,
            L("Permission:Settings"));

        var page = group.AddPermission(SettingsPermissions.Settings, L("Permission:SettingsPage"));
        page.AddChild(SettingsPermissions.Emailing, L("Permission:Emailing"))
            .AddChild(SettingsPermissions.EmailingTest, L("Permission:EmailingTest"));
        page.AddChild(SettingsPermissions.TimeZone, L("Permission:TimeZone"));
        page.AddChild(SettingsPermissions.Identity, L("Permission:Identity"));
        page.AddChild(SettingsPermissions.ExternalAuth, L("Permission:ExternalAuth"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiSettingsResource>(name);
    }
}
