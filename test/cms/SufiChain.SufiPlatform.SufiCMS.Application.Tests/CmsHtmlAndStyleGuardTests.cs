using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Html;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class CmsHtmlAndStyleGuardTests
{
    [Fact]
    public void Annotator_Keeps_Existing_Ids_And_Replaces_Duplicates()
    {
        var annotator = new CmsHtmlAnnotator();
        var first = annotator.Annotate("<section><h1>Hello</h1><p>Lead</p></section>");
        var again = annotator.Annotate(first);
        again.ShouldBe(first);

        var duplicated = first + first;
        var unique = annotator.Annotate(duplicated);
        var sectionIds = System.Text.RegularExpressions.Regex.Matches(unique, "data-cms-section=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToList();
        sectionIds.Count.ShouldBe(2);
        sectionIds.Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public void StripScripts_Removes_Handlers_And_Javascript_Urls()
    {
        var document = CmsHtml.Parse("<section><a href=\"javascript:alert(1)\" onclick=\"evil()\">Go</a><script>alert(1)</script></section>");
        CmsHtml.StripScripts(document);
        var html = CmsHtml.Serialize(document);
        html.ShouldNotContain("script");
        html.ShouldNotContain("onclick");
        html.ShouldNotContain("javascript:");
    }

    [Fact]
    public void StyleGuard_Flags_Literal_Color_Unknown_Class_And_Physical_Property()
    {
        var guard = new CmsStyleGuard();
        var issues = guard.Check(new CmsStyleGuardInput
        {
            Html = "<section class=\"cms-hero mystery\"><p style=\"color:red\">Hi</p></section>",
            PageCss = ".cms-hero { margin-left: 1rem; color: #ff0000; }",
            GlobalCss = ".cms-hero { padding-inline: 1rem; }",
            DesignTokensJson = "{\"--cms-color-text\":\"#111\"}",
            AllowPageLevelStyles = true
        });

        issues.Select(i => i.Code).ShouldContain(CmsStyleGuardCodes.LiteralColor);
        issues.Select(i => i.Code).ShouldContain(CmsStyleGuardCodes.PhysicalProperty);
        issues.Select(i => i.Code).ShouldContain(CmsStyleGuardCodes.UnknownClass);
        issues.Select(i => i.Code).ShouldContain(CmsStyleGuardCodes.InlineStyle);
        issues.Single(i => i.Code == CmsStyleGuardCodes.PhysicalProperty).IsBlocking.ShouldBeFalse();
    }

    [Fact]
    public void StyleGuard_Blocks_Page_Css_When_Disabled()
    {
        var issues = new CmsStyleGuard().Check(new CmsStyleGuardInput
        {
            PageCss = ".x { color: var(--cms-color-text); }",
            AllowPageLevelStyles = false
        });
        issues.ShouldContain(i => i.Code == CmsStyleGuardCodes.PageStylesDisabled && i.IsBlocking);
    }

    [Fact]
    public void Urls_Prefix_Culture_And_Keep_Media_Out_Of_Hooshvare_Library()
    {
        CmsUrls.Localize("/p/about", "fa", "en").ShouldBe("/fa/p/about");
        CmsUrls.Localize("/", "en", "en").ShouldBe("/");
        CMSConsts.MediaStructureKey.ShouldBe("cms-media");
        CMSConsts.MediaUploadsFolder.ShouldNotContain("/AI/Hooshvares/");
        CMSConsts.MediaGeneratedFolder.ShouldNotContain("/AI/Hooshvares/");
    }

    [Fact]
    public void Webhook_Signature_Is_Stable()
    {
        var first = Webhooks.CmsWebhookEvents.Sign("secret", "100", "{\"ok\":true}");
        var second = Webhooks.CmsWebhookEvents.Sign("secret", "100", "{\"ok\":true}");
        first.ShouldBe(second);
        first.ShouldStartWith("sha256=");
        Webhooks.CmsWebhookEvents.Sign("other", "100", "{\"ok\":true}").ShouldNotBe(first);
    }

    [Fact]
    public void ExtractMediaIds_Reads_Cms_Media_Urls()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var html = $"<img src=\"{CmsUrls.Media(id, "hero.png")}\" alt=\"Hero\" />";
        CmsHtml.ExtractMediaIds(html).ShouldBe([id]);
    }

    [Fact]
    public void PatchElement_Keeps_Attributes_And_Strips_Unknown_Tags()
    {
        var editor = new CmsSectionEditor(new CmsHtmlAnnotator());
        var html = """<section data-cms-section="s1"><p class="cms-lead" data-cms-id="e1" dir="rtl">Hello</p></section>""";
        var patched = editor.PatchElement(html, "e1", "<strong>Hi</strong><script>x</script>");
        patched.ShouldContain("data-cms-id=\"e1\"");
        patched.ShouldContain("class=\"cms-lead\"");
        patched.ShouldContain("dir=\"rtl\"");
        patched.ShouldContain("<strong>Hi</strong>");
        patched.ShouldNotContain("<script>");
    }

    [Fact]
    public void ReplaceSection_Keeps_Section_Id_And_Leaves_Siblings()
    {
        var editor = new CmsSectionEditor(new CmsHtmlAnnotator());
        var html = """
            <section data-cms-section="hero"><h1 data-cms-id="h1">A</h1></section>
            <section data-cms-section="about"><p data-cms-id="p1">B</p></section>
            """;
        var result = editor.ReplaceSection(html, "about", "<section class=\"cms-about\"><p>Updated</p></section>");
        result.ShouldContain("data-cms-section=\"hero\"");
        result.ShouldContain("data-cms-section=\"about\"");
        result.ShouldContain("Updated");
        result.ShouldNotContain(">B<");
    }

    [Fact]
    public void Share_Copies_Section_Theme_And_Keeps_The_Other_Language()
    {
        var edited = """
            [data-cms-section="s-1"] {
              background: var(--cms-color-surface);
              font-size: 1.25rem;
              font-family: Inter, sans-serif;
              margin-left: 1rem;
              margin-inline: 1rem;
              --cms-color-primary: var(--cms-color-text);
              --cms-font-family: Inter;
            }
            @font-face { font-family: Inter; src: url(inter.woff2); }
            """;
        var persian = """
            [data-cms-section="s-1"] {
              background: var(--cms-color-background);
              font-family: Vazirmatn, sans-serif;
              direction: rtl;
              margin-left: 0;
              margin-inline: 0;
              --cms-font-family: Vazirmatn;
            }
            @font-face { font-family: Vazirmatn; src: url(vazir.woff2); }
            """;

        var shared = CmsSharedPageAssets.Share(edited, persian);
        shared.ShouldNotBeNull();
        shared.ShouldContain("background: var(--cms-color-surface)");
        shared.ShouldContain("font-size: 1.25rem");
        shared.ShouldContain("margin-inline: 1rem");
        shared.ShouldContain("--cms-color-primary: var(--cms-color-text)");
        shared.ShouldContain("font-family: Vazirmatn, sans-serif");
        shared.ShouldContain("direction: rtl");
        shared.ShouldContain("margin-left: 0");
        shared.ShouldContain("--cms-font-family: Vazirmatn");
        shared.ShouldContain("vazir.woff2");
        shared.ShouldNotContain("Inter");
        shared.ShouldNotContain("margin-left: 1rem");
        shared.ShouldNotContain("margin-inline: 0");
        shared.ShouldNotContain("inter.woff2");
        CmsSharedPageAssets.ChangesOtherCulture(edited, persian).ShouldBeTrue();
    }

    [Fact]
    public void Share_Leaves_The_Other_Language_When_Only_Its_Font_Differs()
    {
        var edited = ".a { color: var(--cms-color-text); font-family: Inter; }";
        var other = ".a { color: var(--cms-color-text); font-family: Vazirmatn; direction: rtl; }";
        CmsSharedPageAssets.ChangesOtherCulture(edited, other).ShouldBeFalse();
    }

    [Fact]
    public void Share_Copies_A_Direction_Selector_And_Drops_Shared_Rules()
    {
        var edited = ":dir(rtl) .a { padding-inline-start: 1rem; font-family: Vazirmatn; }";
        var other = ".a { color: var(--cms-color-text); font-family: Vazirmatn; direction: rtl; }";
        var shared = CmsSharedPageAssets.Share(edited, other);
        shared.ShouldNotBeNull();
        shared.ShouldContain(":dir(rtl)");
        shared.ShouldContain("font-family: Vazirmatn");
        shared.ShouldContain("direction: rtl");
        shared.ShouldNotContain("color:");

        var cleared = CmsSharedPageAssets.Share(null, other);
        cleared.ShouldNotBeNull();
        cleared.ShouldContain("font-family: Vazirmatn");
        cleared.ShouldContain("direction: rtl");
        cleared.ShouldNotContain("color:");
    }
}
