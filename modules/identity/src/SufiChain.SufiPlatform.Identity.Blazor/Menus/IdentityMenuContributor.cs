using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Identity.Blazor.Menus;

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
        var l = context.GetLocalizer<SufiIdentityResource>();
        var administration = context.Menu.GetAdministration();

        var identityMenu = new ApplicationMenuItem(
            IdentityMenuNames.GroupName,
            l["Menu:Identity"],
            icon: "users"
        ).RequirePermissions(SufiChain.SufiPlatform.Identity.IdentityPermissions.Users.Default);

        administration.AddItem(identityMenu);

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.OrganizationUnits,
            l["Menu:OrganizationUnits"],
            url: "/panel/admin/identity/organization-units",
            icon: "folder-tree"
        ).RequirePermissions(IdentityPermissions.OrganizationUnits.Default));

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.Users,
            l["Menu:Users"],
            url: "/panel/admin/identity/users",
            icon: "user"
        ).RequirePermissions(SufiChain.SufiPlatform.Identity.IdentityPermissions.Users.Default));

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.Roles,
            l["Menu:Roles"],
            url: "/panel/admin/identity/roles",
            icon: "shield"
        ).RequirePermissions(SufiChain.SufiPlatform.Identity.IdentityPermissions.Roles.Default));

        identityMenu.AddItem(new ApplicationMenuItem(
            IdentityMenuNames.SecurityLogs,
            l["Menu:SecurityLogs"],
            url: "/panel/admin/identity/security-logs",
            icon: "shield-alert"
        ).RequirePermissions(SufiChain.SufiPlatform.Identity.IdentityPermissions.Users.Default));

        return Task.CompletedTask;
    }
}

/// <summary>
/// Menu name constants for Identity module.
/// </summary>
public static class IdentityMenuNames
{
    public const string GroupName = "SufiIdentity";
    public const string OrganizationUnits = GroupName + ".OrganizationUnits";
    public const string Users = GroupName + ".Users";
    public const string Roles = GroupName + ".Roles";
    public const string SecurityLogs = GroupName + ".SecurityLogs";
}
