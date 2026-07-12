using SufiChain.SufiPlatform.Authorization.Permissions;
using SufiChain.SufiPlatform.Menus.Localization;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Menus.Permissions;

public class MenusPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(MenusPermissions.GroupName, L("Permission:SufiMenus"));
        var menus = group.AddPermission(MenusPermissions.Menus.Default, L("Permission:SufiMenus.Menus"));
        menus.AddChild(MenusPermissions.Menus.Create, L("Permission:SufiMenus.Menus.Create"));
        menus.AddChild(MenusPermissions.Menus.Edit, L("Permission:SufiMenus.Menus.Edit"));
        menus.AddChild(MenusPermissions.Menus.Delete, L("Permission:SufiMenus.Menus.Delete"));
        menus.AddChild(MenusPermissions.Menus.ManageItems, L("Permission:SufiMenus.Menus.ManageItems"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<SufiMenusResource>(name);
}