using SufiChain.SufiPlatform.Localization.Localization;
using SufiChain.SufiPlatform.Localization.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Localization.Blazor.Menus;

/// <summary>
/// Menu contributor for Localization Management module.
/// </summary>
public class LocalizationMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<SufiLocalizationResource>();
        var administrationMenu = context.Menu.GetAdministration();

        var localizationMenu = new ApplicationMenuItem(
            LocalizationMenus.GroupName,
            l["Menu:Localization"],
            icon: "globe"
        ).RequirePermissions(LocalizationPermissions.Texts.Default);

        administrationMenu.AddItem(localizationMenu);

        localizationMenu.AddItem(new ApplicationMenuItem(
            LocalizationMenus.LocalizationTexts,
            l["Menu:LocalizationTexts"],
            url: "/panel/admin/localization-management/texts",
            icon: "file-text"
        ).RequirePermissions(LocalizationPermissions.Texts.Default));

        localizationMenu.AddItem(new ApplicationMenuItem(
            LocalizationMenus.LocalizationResources,
            l["Menu:LocalizationResources"],
            url: "/panel/admin/localization-management/resources",
            icon: "folder"
        ).RequirePermissions(LocalizationPermissions.Resources.Default));
    }
}
