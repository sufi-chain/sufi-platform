using SufiChain.SufiPlatform.Menus.Localization;
using SufiChain.SufiPlatform.Menus.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Menus.Blazor.Menus;

/// <summary>
/// Menu contributor for Menus admin pages.
/// </summary>
public class MenusMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            return ConfigureMainMenuAsync(context);
        }

        return Task.CompletedTask;
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiMenusResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            MenusMenuNames.GroupName,
            l["Menu:SufiMenus"],
            url: "/panel/admin/menu-management/menus",
            icon: "menu",
            order: 21
        ).RequirePermissions(MenusPermissions.Menus.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for Menus module.
/// </summary>
public static class MenusMenuNames
{
    public const string GroupName = "SufiMenus";
    public const string Menus = GroupName + ".Menus";
}