using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.Account.Blazor.Menus;

/// <summary>
/// Menu contributor for Account pages (user menu).
/// </summary>
public class AccountMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.User)
        {
            ConfigureUserMenu(context);
        }
        return Task.CompletedTask;
    }

    private void ConfigureUserMenu(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiAbpIdentityResource>();

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.Profile,
            l["Menu:Profile"],
            url: "/account/profile",
            icon: "user",
            order: 100
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.ChangePassword,
            l["Menu:ChangePassword"],
            url: "/account/change-password",
            icon: "lock",
            order: 200
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.Logout,
            l["Logout"],
            url: "/account/logout",
            icon: "log-out",
            order: int.MaxValue
        ));
    }
}

/// <summary>
/// Menu name constants for Account module.
/// </summary>
public static class AccountMenuNames
{
    public const string GroupName = "Account";
    public const string Profile = GroupName + ".Profile";
    public const string ChangePassword = GroupName + ".ChangePassword";
    public const string Logout = GroupName + ".Logout";
}
