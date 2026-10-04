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
        source.ShouldContain("Values=\"_displayNames\"");
        source.ShouldNotContain("BusinessLocalizationModeSwitch");
        source.ShouldNotContain("BusinessTextEditor");
        source.ShouldNotContain("LocalizationKey=\"_localizationKey\"");
        source.ShouldNotContain("ResourceName=\"_localizationResourceName\"");
        source.ShouldNotContain("LiteralValue=\"_literalDisplayName\"");
        source.ShouldNotContain("متن ثابت");
        source.ShouldNotContain("کلید چندزبانه");
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
        rejected.ErrorMessage.ShouldBe(MenuBusinessTexts.LooksLikeKeyMessage);
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
