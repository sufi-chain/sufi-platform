using System.Globalization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class PublicMenuCultureLocalizationTests
{
    [Fact]
    public void Same_cached_key_localizes_differently_under_ar_es_fa()
    {
        const string key = "SeededMenu:public-default:Item:home:DisplayName";
        string? Lookup(string culture) => culture switch
        {
            "ar" or "ar-SA" => "الرئيسية",
            "es" or "es-ES" => "Inicio",
            "fa" or "fa-IR" => "خانه",
            _ => null
        };

        var ar = MenuDisplayNamePlanner.ResolvePublicLabel(key, "Home", "ar", "fa", Lookup);
        var es = MenuDisplayNamePlanner.ResolvePublicLabel(key, "Home", "es", "fa", Lookup);
        var fa = MenuDisplayNamePlanner.ResolvePublicLabel(key, "Home", "fa", "fa", Lookup);

        ar.ShouldBe("الرئيسية");
        es.ShouldBe("Inicio");
        fa.ShouldBe("خانه");
        es.ShouldNotBe(ar);
        fa.ShouldNotBe(es);
    }

    [Fact]
    public async Task LoadTreeAsync_uses_CurrentUICulture_after_cache()
    {
        var menus = new RecordingCultureMenus();
        using (CultureHelper.Use("ar"))
        {
            await menus.GetTreeAsync("Public", null, "Default");
        }

        using (CultureHelper.Use("es"))
        {
            await menus.GetTreeAsync("Public", null, "Default");
        }

        menus.Cultures.ShouldBe(new[] { "ar", "es" });
    }

    [Fact]
    public void Public_menu_provider_sets_culture_before_get_tree()
    {
        var source = File.ReadAllText(FindHostFile(Path.Combine(
            "Zones",
            "SufiPlatformPublicMenuProvider.cs")));

        var landingCulture = source.IndexOf("var cultureName = ReadRequestCulture", StringComparison.Ordinal);
        var landingUse = source.IndexOf("using (CultureHelper.Use(cultureName))", StringComparison.Ordinal);
        var landingGetTree = source.IndexOf("GetTreeAsync(contextType, contextId, menuName)", StringComparison.Ordinal);
        landingCulture.ShouldBeGreaterThan(-1);
        landingUse.ShouldBeGreaterThan(landingCulture);
        landingGetTree.ShouldBeGreaterThan(landingUse);

        var kbLanguage = source.IndexOf("var language = segments.Length >= 3", StringComparison.Ordinal);
        var kbUse = source.IndexOf("using (CultureHelper.Use(language))", StringComparison.Ordinal);
        kbLanguage.ShouldBeGreaterThan(-1);
        kbUse.ShouldBeGreaterThan(kbLanguage);
    }

    private static string FindHostFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            foreach (var candidate in new[]
                     {
                         Path.Combine(dir.FullName, "hosts", "SufiChane.SufiPlatform", "src", "SufiChane.SufiPlatform.Blazor.WebApp", relativePath),
                         Path.Combine(dir.FullName, "..", "hosts", "SufiChane.SufiPlatform", "src", "SufiChane.SufiPlatform.Blazor.WebApp", relativePath)
                     })
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    return full;
                }
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }

    private sealed class RecordingCultureMenus : PublicMenuAppService
    {
        public List<string> Cultures { get; } = [];

        public RecordingCultureMenus()
            : base(
                Substitute.For<IMenuRepository>(),
                null!,
                null!,
                null!,
                null!,
                Array.Empty<IPublicMenuContextVisibility>())
        {
        }

        protected override Task<List<MenuItemTreeDto>> LocalizeTreeAsync(List<MenuItemTreeDto> items, string contextType)
        {
            Cultures.Add(CultureInfo.CurrentUICulture.Name);
            return Task.FromResult(items);
        }

        protected override Task<List<MenuItemTreeDto>> LoadTreeAsync(string contextType, Guid? contextId, string menuName)
        {
            return LocalizeTreeAsync(
                [new MenuItemTreeDto { DisplayName = "SeededMenu:public-default:Item:home:DisplayName", Name = "home" }],
                contextType);
        }
    }
}
