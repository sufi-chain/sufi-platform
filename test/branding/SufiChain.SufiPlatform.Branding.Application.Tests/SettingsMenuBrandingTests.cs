using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.Branding.Blazor.Menus;
using SufiChain.SufiPlatform.Branding.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Menus;
using SufiChain.SufiPlatform.UI.Navigation;
using Xunit;

namespace SufiChain.SufiPlatform.Branding;

public class SettingsMenuBrandingTests
{
    [Fact]
    public async Task Settings_link_stays_on_the_page_permission()
    {
        var menu = new ApplicationMenu(StandardMenus.Main);
        menu.GetAdministration().AddItem(
            new ApplicationMenuItem(SettingsMenuNames.GroupName, "Settings", "/panel/admin/settings")
                .RequirePermissions(SettingsPermissions.Settings));
        var context = new MenuConfigurationContext(menu, new ServiceCollection().BuildServiceProvider());

        await new BrandingMenuContributor().ConfigureMenuAsync(context);

        var settings = menu.GetAdministration().Items.Single(item => item.Name == SettingsMenuNames.GroupName);
        settings.GetRequiredPermissions().ShouldBe(new[] { SettingsPermissions.Settings });
        settings.GetRequiredPermissions().ShouldNotContain(BrandingPermissions.Branding.Default);
    }
}
