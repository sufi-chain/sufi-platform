using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Menus;

public class SettingsMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiSettingsResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            SettingsMenuNames.GroupName,
            l["Settings"],
            url: "/panel/admin/settings",
            icon: "settings",
            order: 100
        ).RequirePermissions(SettingsPermissions.Settings));

        return Task.CompletedTask;
    }
}

public static class SettingsMenuNames
{
    public const string GroupName = "SufiSettings";
}
