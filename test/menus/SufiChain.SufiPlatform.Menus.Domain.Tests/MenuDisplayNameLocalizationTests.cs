using System.Text.Json;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Menus.Localization;
using SufiChain.SufiPlatform.Menus.Menus;
using Shouldly;
using Volo.Abp.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class MenuDisplayNameLocalizationTests
{
    [Fact]
    public void Literal_mode_stores_the_typed_text()
    {
        BusinessTextEditorStorage.StoreLiteral("  فروشگاه  ").ShouldBe("فروشگاه");
    }

    [Fact]
    public void Key_mode_stores_the_computed_key()
    {
        var computed = BusinessLocalizationKeys.SeededMenuItemDisplayName("public-default", "about");

        BusinessTextEditorStorage.StoreLocalizationKey(computed)
            .ShouldBe("SeededMenu:public-default:Item:about:DisplayName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("_localizationKey")]
    [InlineData(" _localizationKey ")]
    [InlineData("_localizationResourceName")]
    [InlineData("_literalDisplayName")]
    public void Key_mode_rejects_an_empty_or_placeholder_key(string? key)
    {
        Should.Throw<ArgumentException>(() => BusinessTextEditorStorage.StoreLocalizationKey(key));
    }

    [Fact]
    public void Render_falls_back_to_the_plain_name_for_a_placeholder()
    {
        BusinessTextEditorStorage.ResolveDisplayName("_localizationKey", "About", localizedValue: null)
            .ShouldBe("About");
    }

    [Fact]
    public void Render_falls_back_to_the_plain_name_when_the_label_is_empty()
    {
        BusinessTextEditorStorage.ResolveDisplayName("  ", "About", localizedValue: null)
            .ShouldBe("About");
    }

    [Fact]
    public void Render_falls_back_to_the_plain_name_when_the_key_is_unresolved()
    {
        var stored = BusinessLocalizationKeys.SeededMenuItemDisplayName("public-default", "about");

        BusinessTextEditorStorage.ResolveDisplayName(stored, "About", localizedValue: stored)
            .ShouldBe("About");
        BusinessTextEditorStorage.ResolveDisplayName(stored, "About", localizedValue: null)
            .ShouldBe("About");
        BusinessTextEditorStorage.ResolveDisplayName(stored, "About", localizedValue: "  ")
            .ShouldBe("About");
    }

    [Fact]
    public void Render_uses_a_resolved_translation()
    {
        var stored = BusinessLocalizationKeys.SeededMenuItemDisplayName("public-default", "about");

        BusinessTextEditorStorage.ResolveDisplayName(stored, "About", "درباره")
            .ShouldBe("درباره");
    }

    [Fact]
    public void Render_keeps_a_plain_literal_label()
    {
        BusinessTextEditorStorage.ResolveDisplayName("Hello", "About", localizedValue: null)
            .ShouldBe("Hello");
    }

    [Fact]
    public void Public_tree_cache_key_has_no_culture()
    {
        var key = MenuTreeCacheKeys.CreatePublicTreeCacheKey("Public", null, "Default");

        key.ShouldBe("pt:Public:00000000-0000-0000-0000-000000000000:Default");
        MenuTreeCacheKeys.CreatePublicTreeCacheKey("Public", null, "Default").ShouldBe(key);
        key.Split(':').ShouldNotContain("fa");
        key.Split(':').ShouldNotContain("en");
        key.Split(':').ShouldNotContain("ar");
    }

    [Fact]
    public void Public_invalidation_includes_the_tree_key_and_the_item_key()
    {
        var contextId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var keys = MenuTreeCacheKeys.CreatePublicInvalidationKeys("SufiCMS", contextId, "Header", "about");

        keys.Count.ShouldBe(2);
        keys[0].ShouldBe($"pt:SufiCMS:{contextId}:Header");
        keys[1].ShouldBe($"pi:SufiCMS:{contextId}:Header:about");
    }

    [Fact]
    public void Public_invalidation_omits_the_item_key_when_the_slug_is_blank()
    {
        var keys = MenuTreeCacheKeys.CreatePublicInvalidationKeys("Public", null, "Default", " ");

        keys.Count.ShouldBe(1);
        keys[0].ShouldStartWith(MenuTreeCacheKeys.PublicTreePrefix);
    }

    [Theory]
    [InlineData("Pages/MenuItemEditor.razor")]
    [InlineData("Components/MenuCreateModal.razor")]
    [InlineData("Components/MenuEditModal.razor")]
    public void Menu_editors_bind_business_text_values(string relativeFile)
    {
        var source = File.ReadAllText(FindPlatformFile(
            Path.Combine(
                "modules",
                "menus",
                "src",
                "SufiChain.SufiPlatform.Menus.Blazor",
                relativeFile)));

        source.ShouldContain("MultilingualTextField");
        source.ShouldContain("MenuLabel:Advanced");
        source.ShouldContain("SbSwitch");
        source.ShouldContain("Values=\"_displayNames\"");
        if (relativeFile.EndsWith("MenuItemEditor.razor", StringComparison.Ordinal))
        {
            var labels = source.IndexOf("MultilingualTextField", StringComparison.Ordinal);
            var link = source.IndexOf("MenuItemSection:Link", StringComparison.Ordinal);
            var display = source.IndexOf("MenuItemSection:Display", StringComparison.Ordinal);
            var visibility = source.IndexOf("MenuItemSection:Visibility", StringComparison.Ordinal);
            var titleAdvanced = source.IndexOf("MenuLabel:Advanced", StringComparison.Ordinal);
            var linkAdvanced = source.IndexOf("MenuLink:Advanced", StringComparison.Ordinal);
            labels.ShouldBeLessThan(link);
            link.ShouldBeLessThan(display);
            display.ShouldBeLessThan(visibility);
            visibility.ShouldBeLessThan(titleAdvanced);
            titleAdvanced.ShouldBeLessThan(source.IndexOf("MenuName", StringComparison.Ordinal));
            source.IndexOf("MenuName", StringComparison.Ordinal)
                .ShouldBeLessThan(source.IndexOf("Slug", StringComparison.Ordinal));
            titleAdvanced.ShouldBeLessThan(linkAdvanced);
        }

        if (relativeFile.EndsWith("MenuCreateModal.razor", StringComparison.Ordinal))
        {
            source.ShouldContain("SbSimpleSelect");
            source.IndexOf("MenuLabel:Advanced", StringComparison.Ordinal)
                .ShouldBeLessThan(source.IndexOf("ContextType", StringComparison.Ordinal));
            source.ShouldNotContain("ContextTypeExample");
        }

        if (relativeFile.Contains("MenuEditModal.razor", StringComparison.Ordinal) || relativeFile.EndsWith("MenuItemEditor.razor", StringComparison.Ordinal))
        {
            source.ShouldContain("MenuLabel:Copy");
            source.ShouldContain("ReadOnly=\"true\"");
        }
        source.ShouldNotContain("BusinessLocalizationModeSwitch");
        source.ShouldNotContain("BusinessTextEditor");
        source.ShouldNotContain("LocalizationKey=\"_localizationKey\"");
        source.ShouldNotContain("ResourceName=\"_localizationResourceName\"");
        source.ShouldNotContain("LiteralValue=\"_literalDisplayName\"");
        source.ShouldNotContain("متن ثابت");
        source.ShouldNotContain("کلید چندزبانه");
    }

    [Fact]
    public void Persian_title_panel_is_named_apart_from_the_link_panel()
    {
        var texts = ReadMenuTexts("fa.json");
        texts["MenuLabel:Advanced"].ShouldBe("تنظیمات پیشرفتهٔ عنوان");
        texts["MenuLink:Advanced"].ShouldBe("تنظیمات پیشرفته");
        texts["MenuLabel:Advanced"].ShouldNotBe(texts["MenuLink:Advanced"]);
    }

    [Fact]
    public void Item_editor_save_error_is_an_alert_and_uses_the_generic_fallback()
    {
        var markup = File.ReadAllText(FindPlatformFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Blazor",
            "Pages",
            "MenuItemEditor.razor")));
        markup.ShouldContain("id=\"menu-item-save-error\" class=\"menu-item-editor__error\" role=\"alert\" tabindex=\"-1\"");

        var submit = File.ReadAllText(FindPlatformFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Blazor",
            "Pages",
            "MenuItemEditor.razor.cs")));
        submit.ShouldContain("L[\"MenuLink:SaveFailed\"]");
        submit.ShouldNotContain("Sufi.Menus:DisplayNameRequired");
        submit.ShouldContain("PublishSaveError()");

        var links = File.ReadAllText(FindPlatformFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Blazor",
            "Pages",
            "MenuItemEditor.Links.cs")));
        links.ShouldContain("L[\"MenuLink:SaveFailed\"]");
        links.ShouldContain("_focusFieldId = \"menu-item-save-error\"");
        links.ShouldContain("new MenuEditorError(message!, \"menu-item-save-error\")");
    }

    [Fact]
    public void Culture_url_summary_names_the_address_and_the_language()
    {
        var texts = ReadMenuTexts("fa.json");
        texts["MenuLink:CultureUrlSummary"].ShouldBe("نشانی ({0})");
        string.Format(texts["MenuLink:CultureUrlSummary"], "English").ShouldBe("نشانی (English)");

        var links = File.ReadAllText(FindPlatformFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Blazor",
            "Pages",
            "MenuItemEditor.Links.cs")));
        links.ShouldContain("L[\"MenuLink:CultureUrlSummary\", CultureLabel");
        links.ShouldContain("CultureUrlSummary(row.Culture)");
    }

    [Fact]
    public void New_item_keys_use_the_item_id_and_stay_unique_for_the_same_slug()
    {
        var menuId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var first = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var second = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var firstKey = MenuDisplayNamePlanner.ItemKey(menuId, first);
        var secondKey = MenuDisplayNamePlanner.ItemKey(menuId, second);

        firstKey.ShouldBe($"SeededMenu:{menuId:N}:Item:{first:N}:DisplayName");
        secondKey.ShouldNotBe(firstKey);
        firstKey.ShouldNotContain("home");
        MenuDisplayNamePlanner.ResolveStoredKey("خانه", firstKey).ShouldBe(firstKey);
        MenuDisplayNamePlanner.ResolveStoredKey("_localizationKey", firstKey).ShouldBe(firstKey);
    }

    [Fact]
    public void Seeded_item_key_is_kept()
    {
        const string seeded = "SeededMenu:public-default:Item:home:DisplayName";
        var generated = MenuDisplayNamePlanner.ItemKey(Guid.NewGuid(), Guid.NewGuid());

        MenuDisplayNamePlanner.ResolveStoredKey(seeded, generated).ShouldBe(seeded);
    }

    [Fact]
    public void Shared_menu_resource_is_used_for_every_context()
    {
        LocalizationResourceNameAttribute.GetName(typeof(MenuBusinessTextResource)).ShouldBe(MenuBusinessTexts.ResourceName);
        MenuLocalizationRegistry.SharedResourceName.ShouldBe("SufiMenus");
        MenuLocalizationRegistry.GetReadResourceNames(null, "NotRegistered").ShouldBe(new[] { "SufiMenus" });

        MenuLocalizationRegistry.RegisterContextType("SufiCMS", "SufiCMS");
        var names = MenuLocalizationRegistry.GetReadResourceNames("public-default", "SufiCMS");
        names[0].ShouldBe("SufiMenus");
        names.ShouldContain("SufiCMS");
    }

    [Fact]
    public void Write_plan_upserts_filled_cultures_and_deletes_emptied_ones()
    {
        var plan = MenuDisplayNamePlanner.PlanWrite(
            new Dictionary<string, string>
            {
                ["fa"] = "علي",
                ["en"] = "  ",
                ["ar"] = "",
                ["es"] = "Inicio"
            },
            "fa",
            256);

        plan.Succeeded.ShouldBeTrue();
        plan.Upserts["fa"].ShouldBe("علی");
        plan.Upserts["es"].ShouldBe("Inicio");
        plan.Upserts.ContainsKey("en").ShouldBeFalse();
        plan.Deletes.ShouldContain("en");
        plan.Deletes.ShouldContain("ar");
    }

    [Fact]
    public void Write_plan_keeps_zwnj_and_rejects_a_key_like_value()
    {
        var kept = MenuDisplayNamePlanner.PlanWrite(
            new Dictionary<string, string> { ["fa"] = "می‌روم" },
            "fa",
            256);
        kept.Upserts["fa"].ShouldBe("می‌روم");

        var rejected = MenuDisplayNamePlanner.PlanWrite(
            new Dictionary<string, string> { ["fa"] = "_localizationKey" },
            "fa",
            256);
        rejected.Succeeded.ShouldBeFalse();
        rejected.ErrorCode.ShouldBe(MenuBusinessTexts.ErrorLooksLikeKey);
    }

    [Fact]
    public void Clearing_a_seeded_language_writes_nothing_and_keeps_the_base_text()
    {
        var plan = MenuDisplayNamePlanner.PlanWrite(
            new Dictionary<string, string>
            {
                ["fa"] = "خانه",
                ["en"] = "Home"
            },
            "fa",
            256);

        MenuDisplayNamePlanner.ReleaseBaseCopies(plan, new Dictionary<string, string> { ["en"] = "Home" });

        plan.Succeeded.ShouldBeTrue();
        plan.Upserts["fa"].ShouldBe("خانه");
        plan.Upserts.ContainsKey("en").ShouldBeFalse();
        plan.Deletes.ShouldContain("en");

        const string key = "SeededMenu:public-default:Item:home:DisplayName";
        var shown = MenuDisplayNamePlanner.ResolvePublicLabel(
            key,
            "خانه",
            "en",
            "fa",
            culture => culture == "en" ? "Home" : culture == "fa" ? "خانه" : null);
        shown.ShouldBe("Home");
    }

    [Fact]
    public void Editor_state_keeps_seeded_texts_and_clears_a_placeholder()
    {
        const string seeded = "SeededMenu:public-default:Item:home:DisplayName";
        var cultures = new[] { "fa", "en", "ar", "es" };
        var seededState = MenuDisplayNamePlanner.BuildEditorState(
            seeded,
            "خانه",
            cultures,
            "fa",
            culture => culture switch
            {
                "fa" => "خانه",
                "en" => "Home",
                "ar" => "الرئيسية",
                "es" => "Inicio",
                _ => null
            });

        seededState.PreservedKey.ShouldBe(seeded);
        seededState.Values["fa"].ShouldBe("خانه");
        seededState.Values["en"].ShouldBe("Home");
        seededState.Values["ar"].ShouldBe("الرئيسية");
        seededState.Values["es"].ShouldBe("Inicio");

        var broken = MenuDisplayNamePlanner.BuildEditorState("_localizationKey", "About", cultures, "fa", _ => "ignored");
        broken.BrokenPlaceholder.ShouldBeTrue();
        broken.Values.Values.ShouldAllBe(value => string.IsNullOrEmpty(value));
        broken.Suggestion.ShouldBe("About");
        broken.PreservedKey.ShouldBeNull();
    }

    [Fact]
    public void Literal_label_opens_in_the_default_culture()
    {
        var state = MenuDisplayNamePlanner.BuildEditorState(
            "فروشگاه",
            "Shop",
            new[] { "fa", "en" },
            "fa",
            _ => null);

        state.PreservedKey.ShouldBeNull();
        state.Values["fa"].ShouldBe("فروشگاه");
        state.Values["en"].ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("ar")]
    [InlineData("es")]
    [InlineData("fa")]
    [InlineData("fa-IR")]
    public void Public_fallback_uses_the_default_culture_and_never_the_key(string requested)
    {
        const string key = "SeededMenu:public-default:Item:home:DisplayName";
        var label = MenuDisplayNamePlanner.ResolvePublicLabel(
            key,
            "Home",
            requested,
            "fa",
            culture => culture is "fa" or "fa-IR" ? "خانه" : null);

        label.ShouldBe("خانه");
        label.ShouldNotStartWith("SeededMenu:");
        label.ShouldNotStartWith("_");
    }

    [Fact]
    public void Public_fallback_hides_when_the_name_is_missing_too()
    {
        const string key = "SeededMenu:public-default:Item:home:DisplayName";
        MenuDisplayNamePlanner.ResolvePublicLabel(key, "  ", "en", "fa", _ => key).ShouldBe(string.Empty);
        MenuDisplayNamePlanner.ResolvePublicLabel("_localizationKey", "About", "fa", "fa", _ => "خانه").ShouldBe("About");
    }

    [Fact]
    public void Public_resolution_does_not_change_the_cached_key()
    {
        const string cached = "SeededMenu:public-default:Item:home:DisplayName";
        var en = MenuDisplayNamePlanner.ResolvePublicLabel(cached, "Home", "en", "fa", culture => culture == "en" ? "Home" : "خانه");
        var fa = MenuDisplayNamePlanner.ResolvePublicLabel(cached, "Home", "fa", "fa", culture => culture == "en" ? "Home" : "خانه");

        en.ShouldBe("Home");
        fa.ShouldBe("خانه");
        cached.ShouldBe("SeededMenu:public-default:Item:home:DisplayName");
        MenuTreeCacheKeys.CreatePublicTreeCacheKey("Public", null, "Default").Split(':').ShouldNotContain("fa");
    }

    [Fact]
    public void Exact_lookup_keeps_a_cleared_seeded_language_instead_of_the_default_text()
    {
        const string key = "SeededMenu:public-default:Item:home:DisplayName";
        var options = new AbpLocalizationOptions();
        var shared = options.Resources.Add<MenuBusinessTextResource>("fa");
        shared.Contributors.Add(new CultureMapContributor(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "خانه"
        }));

        var seeded = options.Resources.Add<SeededMenuLookupResource>("fa");
        seeded.Contributors.Add(new CultureMapContributor(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "خانه",
            ["en"] = "Home"
        }));

        var resources = new[] { "SufiMenus", "SeededMenus" };
        MenuDisplayNamePlanner.LookupExact(options, resources, key, "en").ShouldBe("Home");
        MenuDisplayNamePlanner.LookupExact(options, resources, key, "fa").ShouldBe("خانه");

        var label = MenuDisplayNamePlanner.ResolvePublicLabel(
            key,
            "خانه",
            "en",
            "fa",
            culture => MenuDisplayNamePlanner.LookupExact(options, resources, key, culture));
        label.ShouldBe("Home");
    }

    [LocalizationResourceName("SeededMenus")]
    private sealed class SeededMenuLookupResource
    {
    }

    private sealed class CultureMapContributor : ILocalizationResourceContributor
    {
        private readonly Dictionary<string, string> _values;

        public CultureMapContributor(Dictionary<string, string> values)
        {
            _values = values;
        }

        public bool IsDynamic => false;

        public void Initialize(LocalizationResourceInitializationContext context)
        {
        }

        public LocalizedString? GetOrNull(string cultureName, string name)
        {
            return _values.TryGetValue(cultureName, out var value)
                ? new LocalizedString(name, value)
                : null;
        }

        public void Fill(string cultureName, Dictionary<string, LocalizedString> dictionary)
        {
        }

        public Task FillAsync(string cultureName, Dictionary<string, LocalizedString> dictionary) =>
            Task.CompletedTask;

        public Task<IEnumerable<string>> GetSupportedCulturesAsync() =>
            Task.FromResult<IEnumerable<string>>(_values.Keys);
    }

    private static Dictionary<string, string> ReadMenuTexts(string fileName)
    {
        var path = FindPlatformFile(Path.Combine(
            "modules",
            "menus",
            "src",
            "SufiChain.SufiPlatform.Menus.Domain.Shared",
            "Localization",
            "Menus",
            fileName));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.GetProperty("texts").EnumerateObject())
        {
            texts[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return texts;
    }

    private static string FindPlatformFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
