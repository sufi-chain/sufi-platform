using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Menus.Menus;
using Shouldly;
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

        source.ShouldContain("ResourceName=\"@_localizationResourceName\"");
        source.ShouldContain("LocalizationKey=\"@_localizationKey\"");
        source.ShouldContain("LiteralValue=\"@_literalDisplayName\"");
        source.ShouldNotContain("ResourceName=\"_localizationResourceName\"");
        source.ShouldNotContain("LocalizationKey=\"_localizationKey\"");
        source.ShouldNotContain("LiteralValue=\"_literalDisplayName\"");
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
