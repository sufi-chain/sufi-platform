using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Localization;
using SufiChain.SufiAbp.MenuManagement.Localization;

namespace SufiChain.SufiAbp.MenuManagement.Permissions;

public class MenuManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(MenuManagementPermissions.GroupName, L("Permission:MenuManagement"));
        var menus = group.AddPermission(MenuManagementPermissions.Menus.Default, L("Permission:MenuManagement.Menus"));
        menus.AddChild(MenuManagementPermissions.Menus.Create, L("Permission:MenuManagement.Menus.Create"));
        menus.AddChild(MenuManagementPermissions.Menus.Edit, L("Permission:MenuManagement.Menus.Edit"));
        menus.AddChild(MenuManagementPermissions.Menus.Delete, L("Permission:MenuManagement.Menus.Delete"));
        menus.AddChild(MenuManagementPermissions.Menus.ManageItems, L("Permission:MenuManagement.Menus.ManageItems"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<SufiAbpMenuManagementResource>(name);
}
