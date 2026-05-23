using SufiChain.SufiAbp.SettingManagement.Localization;
using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Localization;

namespace SufiChain.SufiAbp.SettingManagement;

public class SettingManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            SettingManagementPermissions.GroupName,
            L("Permission:SettingManagement"));

        group.AddPermission(SettingManagementPermissions.Emailing, L("Permission:Emailing"))
            .AddChild(SettingManagementPermissions.EmailingTest, L("Permission:EmailingTest"));
        group.AddPermission(SettingManagementPermissions.TimeZone, L("Permission:TimeZone"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpSettingManagementResource>(name);
    }
}
