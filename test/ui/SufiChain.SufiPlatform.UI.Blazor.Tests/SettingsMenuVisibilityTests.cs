using Shouldly;
using SufiChain.SufiPlatform.UI.Navigation;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SettingsMenuVisibilityTests
{
    [Fact]
    public async Task Menu_SettingsPagePlusOneSection_ShowsSettingsUnderAdministration()
    {
        var manager = DefaultMenuManagerFeatureFilteringTests.BuildGranted(
            ["SufiSettings.Settings", "SufiSettings.Localization"],
            menu =>
            {
                menu.GetAdministration().AddItem(
                    new ApplicationMenuItem("SufiSettings", "Settings", "/panel/admin/settings")
                        .RequirePermissions("SufiSettings.Settings"));
            });

        var result = await manager.GetMainMenuAsync();
        var administration = result.Items.Single(item => item.Name == "Administration");
        administration.Items.ShouldContain(item => item.Url == "/panel/admin/settings");
    }

    [Fact]
    public async Task Menu_NoSettingsPermission_HidesSettings()
    {
        var manager = DefaultMenuManagerFeatureFilteringTests.BuildGranted(
            ["SufiSettings.Localization"],
            menu =>
            {
                menu.GetAdministration().AddItem(
                    new ApplicationMenuItem("SufiSettings", "Settings", "/panel/admin/settings")
                        .RequirePermissions("SufiSettings.Settings"));
            });

        var result = await manager.GetMainMenuAsync();
        result.Items.SelectMany(item => item.Items).ShouldNotContain(item => item.Url == "/panel/admin/settings");
    }
}
