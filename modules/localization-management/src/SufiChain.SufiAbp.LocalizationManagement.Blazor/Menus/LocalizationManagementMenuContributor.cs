using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.LocalizationManagement.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.LocalizationManagement.Blazor.Menus;

/// <summary>
/// Menu contributor for Localization Management module.
/// </summary>
public class LocalizationManagementMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            ConfigureMainMenu(context);
        }
        return Task.CompletedTask;
    }

    private static void ConfigureMainMenu(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiAbpLocalizationManagementResource>();
        var administrationMenu = context.Menu.GetAdministration();

        var localizationMenu = new ApplicationMenuItem(
            LocalizationManagementMenus.GroupName,
            l["Menu:LocalizationManagement"],
            icon: "globe"
        ).RequirePermissions(LocalizationManagementPermissions.Texts.Default);

        administrationMenu.AddItem(localizationMenu);

        localizationMenu.AddItem(new ApplicationMenuItem(
            LocalizationManagementMenus.LocalizationTexts,
            l["Menu:LocalizationTexts"],
            url: "/admin/localization-management/texts",
            icon: "file-text"
        ).RequirePermissions(LocalizationManagementPermissions.Texts.Default));

        localizationMenu.AddItem(new ApplicationMenuItem(
            LocalizationManagementMenus.LocalizationResources,
            l["Menu:LocalizationResources"],
            url: "/admin/localization-management/resources",
            icon: "folder"
        ).RequirePermissions(LocalizationManagementPermissions.Resources.Default));
    }
}
