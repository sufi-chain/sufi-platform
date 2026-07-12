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

        group.AddPermission(SettingsPermissions.Emailing, L("Permission:Emailing"))
            .AddChild(SettingsPermissions.EmailingTest, L("Permission:EmailingTest"));
        group.AddPermission(SettingsPermissions.TimeZone, L("Permission:TimeZone"));
        group.AddPermission(SettingsPermissions.Identity, L("Permission:Identity"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiSettingsResource>(name);
    }
}
