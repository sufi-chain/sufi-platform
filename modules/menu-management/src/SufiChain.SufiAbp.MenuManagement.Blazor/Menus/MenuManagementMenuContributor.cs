using SufiChain.SufiAbp.MenuManagement.Localization;
using SufiChain.SufiAbp.MenuManagement.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Menus;

/// <summary>
/// Menu contributor for MenuManagement admin pages.
/// </summary>
public class MenuManagementMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiAbpMenuManagementResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            MenuManagementMenuNames.GroupName,
            l["Menu:MenuManagement"],
            url: "/panel/admin/menu-management/menus",
            icon: "menu",
            order: 21
        ).RequirePermissions(MenuManagementPermissions.Menus.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for MenuManagement module.
/// </summary>
public static class MenuManagementMenuNames
{
    public const string GroupName = "MenuManagement";
    public const string Menus = GroupName + ".Menus";
}
