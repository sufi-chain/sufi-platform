using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.Identity.Blazor.Menus;

/// <summary>
/// Menu contributor for Identity management pages.
/// </summary>
public class IdentityMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiAbpIdentityResource>();
        var administration = context.Menu.GetAdministration();

        var identityMenu = new ApplicationMenuItem(
            IdentityMenuNames.GroupName,
            l["Menu:Identity"],
            icon: "users"
        ).RequirePermissions(SufiChain.SufiAbp.Identity.IdentityPermissions.Users.Default);

        administration.AddItem(identityMenu);

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.OrganizationUnits,
            l["Menu:OrganizationUnits"],
            url: "/admin/identity/organization-units",
            icon: "folder-tree"
        ).RequirePermissions(IdentityPermissions.OrganizationUnits.Default));

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.Users,
            l["Menu:Users"],
            url: "/admin/identity/users",
            icon: "user"
        ).RequirePermissions(SufiChain.SufiAbp.Identity.IdentityPermissions.Users.Default));

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.Roles,
            l["Menu:Roles"],
            url: "/admin/identity/roles",
            icon: "shield"
        ).RequirePermissions(SufiChain.SufiAbp.Identity.IdentityPermissions.Roles.Default));

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.SecurityLogs,
            l["Menu:SecurityLogs"],
            url: "/admin/identity/security-logs",
            icon: "shield-alert"
        ).RequirePermissions(SufiChain.SufiAbp.Identity.IdentityPermissions.Users.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for Identity module.
/// </summary>
public static class IdentityMenuNames
{
    public const string GroupName = "Identity";
    public const string OrganizationUnits = GroupName + ".OrganizationUnits";
    public const string Users = GroupName + ".Users";
    public const string Roles = GroupName + ".Roles";
    public const string SecurityLogs = GroupName + ".SecurityLogs";
}
