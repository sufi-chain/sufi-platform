using SufiChain.SufiAbp.SettingManagement.Localization;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.Blazor.Menus;

public class SettingManagementMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiAbpSettingManagementResource>();
        var administration = context.Menu.GetAdministration();

        // Show settings menu if user has permission to any setting group
        administration.AddItem(new ApplicationMenuItem(
            SettingManagementMenuNames.GroupName,
            l["Settings"],
            url: "/admin/settings",
            icon: "settings",
            order: 100
        ).RequirePermissions(
            SettingManagementPermissions.Emailing, 
            SettingManagementPermissions.TimeZone));

        return Task.CompletedTask;
    }
}

public static class SettingManagementMenuNames
{
    public const string GroupName = "SettingManagement";
}
