using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Features;
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

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var featureChecker = context.ServiceProvider.GetRequiredService<IFeatureChecker>();

        if (!await featureChecker.IsEnabledAsync(SufiAbpShortLinkGeneratorFeatures.Enable) ||
            !await featureChecker.IsEnabledAsync(SufiAbpShortLinkGeneratorFeatures.ShortLinks))
        {
            return;
        }

        var administrationMenu = context.Menu.GetAdministration();
        var l = context.GetLocalizer<SufiAbpShortLinkGeneratorResource>();
        
        administrationMenu.AddItem(new ApplicationMenuItem(
            ShortLinkGeneratorMenus.ShortLinks,
            l["Menu:ShortLinks"],
            url: "/short-link/short-links",
            icon: "fa fa-link",
            requiredPermissionName: ShortLinkGeneratorPermissions.ShortLinks.Default
        ));

        return;
    }
}
