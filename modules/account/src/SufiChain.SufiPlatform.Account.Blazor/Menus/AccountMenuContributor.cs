using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Account.Blazor.Menus;

/// <summary>
/// Menu contributor for Account pages (user menu + portal profile).
/// </summary>
public class AccountMenuContributor : IMenuContributor
{
    public const string ProfileBaseUrl = "/panel/portal/profile";

    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            ConfigureMainMenu(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            ConfigureUserMenu(context);
        }

        return Task.CompletedTask;
    }

    private void ConfigureMainMenu(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiIdentityResource>();

        context.Menu.GetPortal().AddItem(new ApplicationMenuItem(
            AccountMenuNames.PortalProfile,
            l["Menu:Profile"],
            url: ProfileBaseUrl,
            icon: "user",
            order: 5
        ).RequireAuthenticated());
    }

    private void ConfigureUserMenu(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiIdentityResource>();
        var accountL = context.GetLocalizer<SufiAccountResource>();

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.Dashboard,
            accountL["Menu:Dashboard"],
            url: "/panel/dashboard",
            icon: "home",
            order: 10
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.ProfileDivider,
            "-",
            order: 50
        ).AsDivider());

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.Profile,
            l["Menu:Profile"],
            url: $"{ProfileBaseUrl}?tab=profile",
            icon: "user",
            order: 100
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.ChangePassword,
            l["Menu:ChangePassword"],
            url: $"{ProfileBaseUrl}?tab=password",
            icon: "lock",
            order: 200
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.TwoFactor,
            accountL["Menu:TwoFactor"],
            url: $"{ProfileBaseUrl}?tab=two-factor",
            icon: "shield",
            order: 250
        ));

        context.Menu.AddItem(new ApplicationMenuItem(
            AccountMenuNames.LinkedAccounts,
            accountL["Menu:LinkedAccounts"],
            url: $"{ProfileBaseUrl}?tab=linked-accounts",
            icon: "link",
            order: 260
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
    public const string GroupName = "SufiAccount";
    public const string PortalProfile = GroupName + ".Portal.Profile";
    public const string Dashboard = GroupName + ".Dashboard";
    public const string ProfileDivider = GroupName + ".ProfileDivider";
    public const string Profile = GroupName + ".Profile";
    public const string ChangePassword = GroupName + ".ChangePassword";
    public const string TwoFactor = GroupName + ".TwoFactor";
    public const string LinkedAccounts = GroupName + ".LinkedAccounts";
    public const string Logout = GroupName + ".Logout";
}
