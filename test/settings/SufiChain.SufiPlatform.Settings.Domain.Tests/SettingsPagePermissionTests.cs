using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class SettingsPagePermissionTests
{
    [Fact]
    public void Page_permission_is_the_parent_of_each_section()
    {
        var context = new PermissionDefinitionContext(new ServiceCollection().BuildServiceProvider());
        new SettingsPermissionDefinitionProvider().Define(context);

        var page = context.GetPermissionOrNull(SettingsPermissions.Settings);
        page.ShouldNotBeNull();
        var childNames = page!.Children.Select(child => child.Name).ToList();
        childNames.Count.ShouldBe(4);
        childNames.ShouldContain(SettingsPermissions.Emailing);
        childNames.ShouldContain(SettingsPermissions.TimeZone);
        childNames.ShouldContain(SettingsPermissions.Identity);
        childNames.ShouldContain(SettingsPermissions.ExternalAuth);

        var emailing = context.GetPermissionOrNull(SettingsPermissions.Emailing);
        emailing.ShouldNotBeNull();
        emailing!.Parent!.Name.ShouldBe(SettingsPermissions.Settings);
        emailing.Children.Select(child => child.Name).ShouldContain(SettingsPermissions.EmailingTest);
    }

    [Theory]
    [InlineData("fa", "صفحه تنظیمات")]
    [InlineData("en", "Settings page")]
    [InlineData("ar", "صفحة الإعدادات")]
    [InlineData("es", "Página de ajustes")]
    public void Page_permission_label_is_localized(string culture, string label)
    {
        var json = ReadRepoFile(
            "sufi-platform",
            "modules",
            "settings",
            "src",
            "SufiChain.SufiPlatform.Settings.Domain.Shared",
            "Localization",
            "Settings",
            culture + ".json");

        json.ShouldContain($"\"Permission:SettingsPage\": \"{label}\"");
    }

    [Fact]
    public void Settings_page_and_menu_require_the_page_permission()
    {
        var page = ReadRepoFile(
            "sufi-platform",
            "modules",
            "settings",
            "src",
            "SufiChain.SufiPlatform.Settings.Blazor",
            "Pages",
            "SettingsManagement.razor");
        var menu = ReadRepoFile(
            "sufi-platform",
            "modules",
            "settings",
            "src",
            "SufiChain.SufiPlatform.Settings.Blazor",
            "Menus",
            "SettingsMenuContributor.cs");

        page.ShouldContain("[Authorize(SettingsPermissions.Settings)]");
        menu.ShouldContain("RequirePermissions(SettingsPermissions.Settings)");
        menu.ShouldNotContain("SettingsPermissions.Emailing");
    }

    private static string ReadRepoFile(params string[] relativeParts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine([dir.FullName, .. relativeParts]);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(Path.Combine(relativeParts));
    }
}
