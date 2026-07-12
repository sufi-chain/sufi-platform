using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.Features;
using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.ShortLinks.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor.Menus;

public class ShortLinksMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var featureChecker = context.ServiceProvider.GetRequiredService<IFeatureChecker>();

        if (!await featureChecker.IsEnabledAsync(SufiShortLinksFeatures.Enable) ||
            !await featureChecker.IsEnabledAsync(SufiShortLinksFeatures.ShortLinks))
        {
            return;
        }

        var administrationMenu = context.Menu.GetAdministration();
        var l = context.GetLocalizer<SufiShortLinksResource>();
        
        administrationMenu.AddItem(new ApplicationMenuItem(
            ShortLinksMenus.ShortLinks,
            l["Menu:SufiShortLinks"],
            url: "/panel/short-link/short-links",
            icon: "link",
            requiredPermissionName: ShortLinksPermissions.ShortLinks.Default
        ));

        return;
    }
}