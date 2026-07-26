using SufiChain.SufiPlatform.Editions.Localization;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Editions.Blazor.Menus;

public class EditionsMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return Task.CompletedTask;
        }

        var l = context.GetLocalizer<EditionsResource>();
        var administration = context.Menu.GetAdministration();

        administration.AddItem(new ApplicationMenuItem(
            "SufiEditions.Editions",
            l["Menu:Editions"],
            url: "/panel/admin/editions",
            icon: "layers",
            order: 15
        ).RequirePermissions(EditionsPermissions.Editions.Default));

        return Task.CompletedTask;
    }
}
