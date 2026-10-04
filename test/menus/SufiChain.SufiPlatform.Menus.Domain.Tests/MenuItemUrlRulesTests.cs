using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class MenuItemUrlRulesTests
{
    [Theory]
    [InlineData("/about", "/about")]
    [InlineData("  /about?x=1#section  ", "/about?x=1#section")]
    [InlineData("#section", "#section")]
    [InlineData("#", "#")]
    [InlineData("https://example.com/Path", "https://example.com/Path")]
    [InlineData("HTTPS://Example.com/Path", "https://Example.com/Path")]
    [InlineData("http://example.com", "http://example.com")]
    [InlineData("mailto:user@example.com", "mailto:user@example.com")]
    [InlineData("MAILTO:user@example.com?subject=Hi", "mailto:user@example.com?subject=Hi")]
    [InlineData("tel:+1-555-0100", "tel:+1-555-0100")]
    [InlineData("TEL:+15551212", "tel:+15551212")]
    public void Normalize_Should_Accept_Safe_Urls(string input, string expected)
    {
        MenuItemUrlRules.Normalize(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData(" JAVASCRIPT:alert(1)")]
    [InlineData("data:text/html,hi")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("file:///etc/passwd")]
    [InlineData("blob:https://example.com/id")]
    [InlineData("ftp://example.com")]
    [InlineData("//example.com")]
    [InlineData("/\\example.com")]
    [InlineData("https://")]
    [InlineData("https:///no-host")]
    [InlineData("mailto:")]
    [InlineData("tel:")]
    [InlineData("about")]
    [InlineData("https://example.com/a b")]
    [InlineData("<script>")]
    public void Normalize_Should_Reject_Unsafe_Urls(string input)
    {
        MenuItemUrlRules.Normalize(input).ShouldBeNull();
        MenuItemUrlRules.IsExternal(MenuItemUrlRules.Normalize(input)).ShouldBeFalse();
    }

    [Theory]
    [InlineData(MenuItemKind.InternalRoute, "#top", true)]
    [InlineData(MenuItemKind.InternalRoute, "/docs", true)]
    [InlineData(MenuItemKind.InternalRoute, "https://example.com", true)]
    [InlineData(MenuItemKind.InternalRoute, "javascript:alert(1)", false)]
    [InlineData(MenuItemKind.InternalRoute, null, false)]
    [InlineData(MenuItemKind.ExternalUrl, "mailto:user@example.com", true)]
    [InlineData(MenuItemKind.ExternalUrl, "tel:+1555", true)]
    [InlineData(MenuItemKind.ExternalUrl, "/only-a-path", true)]
    [InlineData(MenuItemKind.ExternalUrl, null, false)]
    [InlineData(MenuItemKind.Container, null, true)]
    [InlineData(MenuItemKind.Container, "javascript:alert(1)", false)]
    public void IsAcceptable_Should_Follow_Kind_And_Scheme(MenuItemKind kind, string? url, bool expected)
    {
        MenuItemUrlRules.IsAcceptable(kind, url).ShouldBe(expected);
    }

    [Fact]
    public void IsExternal_Should_Be_True_Only_For_Absolute_Schemes()
    {
        MenuItemUrlRules.IsExternal("https://example.com").ShouldBeTrue();
        MenuItemUrlRules.IsExternal("http://example.com").ShouldBeTrue();
        MenuItemUrlRules.IsExternal("mailto:user@example.com").ShouldBeTrue();
        MenuItemUrlRules.IsExternal("tel:+1555").ShouldBeTrue();
        MenuItemUrlRules.IsExternal("/about").ShouldBeFalse();
        MenuItemUrlRules.IsExternal("#section").ShouldBeFalse();
    }

    [Fact]
    public void Resolve_Should_Prefer_Exact_Culture_Then_Parent_Then_Default()
    {
        var urls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = "/fa/about",
            ["fa-IR"] = "https://example.com/ir",
            ["en"] = "#english"
        };

        MenuItemUrlRules.Resolve("/default", urls, "fa-IR").ShouldBe("https://example.com/ir");
        MenuItemUrlRules.Resolve("/default", urls, "fa").ShouldBe("/fa/about");
        MenuItemUrlRules.Resolve("/default", urls, "fa-AF").ShouldBe("/fa/about");
        MenuItemUrlRules.Resolve("/default", urls, "en-US").ShouldBe("#english");
        MenuItemUrlRules.Resolve("/default", urls, "de-DE").ShouldBe("/default");
        MenuItemUrlRules.Resolve("/default", urls, null).ShouldBe("/default");
        MenuItemUrlRules.Resolve("HTTPS://example.com/home", null, "fa").ShouldBe("https://example.com/home");
    }

    [Fact]
    public void Resolve_Should_Ignore_Empty_Or_Unsafe_Overrides()
    {
        var urls = new Dictionary<string, string>
        {
            ["fa"] = "   ",
            ["en"] = "javascript:alert(1)"
        };

        MenuItemUrlRules.Resolve("/default", urls, "fa-IR").ShouldBe("/default");
        MenuItemUrlRules.Resolve("/default", urls, "en").ShouldBe("/default");
        MenuItemUrlRules.Resolve("javascript:alert(1)", urls, "de").ShouldBeNull();
    }

    [Fact]
    public void Culture_Url_Text_Should_Round_Trip_And_Canonicalize()
    {
        var parsed = MenuItemUrlRules.TryParseCultureUrls("FA_ir https://Example.com/fa\nen #top\n", out var urls);
        parsed.ShouldBeTrue();
        urls["fa-IR"].ShouldBe("https://Example.com/fa");
        urls["en"].ShouldBe("#top");
        MenuItemUrlRules.Resolve("/default", urls, "fa_IR").ShouldBe("https://Example.com/fa");

        var text = MenuItemUrlRules.FormatCultureUrls(urls);
        text.ShouldBe("en #top\nfa-IR https://Example.com/fa");
        MenuItemUrlRules.DeserializeCultureUrls(MenuItemUrlRules.SerializeCultureUrls(urls))["fa-IR"]
            .ShouldBe("https://Example.com/fa");
    }

    [Theory]
    [InlineData("en javascript:alert(1)")]
    [InlineData("not a culture")]
    [InlineData("fa")]
    public void TryParseCultureUrls_Should_Reject_Invalid_Lines(string text)
    {
        MenuItemUrlRules.TryParseCultureUrls(text, out var urls).ShouldBeFalse();
        urls.ShouldBeEmpty();
    }

    [Fact]
    public void MenuItem_Should_Store_Culture_Urls_In_ExtraProperties_And_Fall_Back()
    {
        var item = new MenuItem(Guid.NewGuid(), Guid.NewGuid(), "about", "About", "about");
        item.SetKind(MenuItemKind.ExternalUrl);
        item.SetLink("HTTPS://example.com/default", MenuLinkTarget.SameTab);
        item.Url.ShouldBe("https://example.com/default");

        item.SetCultureUrls(new Dictionary<string, string>
        {
            ["fa_IR"] = "mailto:fa@example.com",
            ["en"] = "#top"
        });

        item.ExtraProperties[MenuItemUrlRules.CultureUrlsPropertyName].ShouldBe("en #top\nfa-IR mailto:fa@example.com");
        item.ResolveUrl("fa-IR").ShouldBe("mailto:fa@example.com");
        item.ResolveUrl("fa").ShouldBe("https://example.com/default");
        item.ResolveUrl("en-US").ShouldBe("#top");

        item.SetCultureUrls(new Dictionary<string, string>());
        item.GetCultureUrls().ShouldBeEmpty();
        item.ExtraProperties.ContainsKey(MenuItemUrlRules.CultureUrlsPropertyName).ShouldBeFalse();
        item.ResolveUrl("fa-IR").ShouldBe("https://example.com/default");
    }

    [Fact]
    public void MenuItem_Should_Reject_Unsafe_Links_Without_Replacing_Stored_Overrides()
    {
        var item = new MenuItem(Guid.NewGuid(), Guid.NewGuid(), "about", "About", "about");
        item.SetKind(MenuItemKind.InternalRoute);
        item.SetLink("#section", MenuLinkTarget.SameTab);
        item.SetCultureUrls(new Dictionary<string, string> { ["en"] = "/en/about" });

        var error = Should.Throw<BusinessException>(() => item.SetLink("javascript:alert(1)", MenuLinkTarget.NewTab));
        error.Code.ShouldBe(MenusErrorCodes.MenuItemInvalidUrl);
        item.Url.ShouldBe("#section");

        var cultureError = Should.Throw<BusinessException>(() =>
            item.SetCultureUrls(new Dictionary<string, string> { ["en"] = "javascript:alert(1)" }));
        cultureError.Code.ShouldBe(MenusErrorCodes.MenuItemInvalidUrl);
        item.GetCultureUrls()["en"].ShouldBe("/en/about");
    }
}
