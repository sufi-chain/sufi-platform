using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.ShortLinkGenerator.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Menus;

public class ShortLinkGeneratorMenuContributor : IMenuContributor
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
        var administrationMenu = context.Menu.GetAdministration();
        var l = context.GetLocalizer<SufiAbpShortLinkGeneratorResource>();
        
        // Add Short Link Generator menu item to Administration menu
        administrationMenu.AddItem(new ApplicationMenuItem(
            ShortLinkGeneratorMenus.ShortLinks,
            l["Menu:ShortLinks"],
            url: "/short-link/short-links",
            icon: "fa fa-link",
            requiredPermissionName: ShortLinkGeneratorPermissions.ShortLinks.Default
        ));

        return Task.CompletedTask;
    }
}

