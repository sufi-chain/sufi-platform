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
    public void Share_Copies_Font_Face_Direction_And_Physical_Edges()
    {
        var edited = """
            @font-face {
              font-family: "Vazirmatn";
              src: url("/fonts/vazirmatn.woff2") format("woff2");
              unicode-range: U+0600-06FF;
            }
            .hero {
              font-family: "Vazirmatn", sans-serif;
              direction: rtl;
              margin-left: 1rem;
              margin-right: 2rem;
              left: 0;
              right: 4px;
              text-align: right;
            }
            .hero::before { content: "سلام"; }
            .hero::after { content: "مرحبا"; }
            """;
        var truncated = ".hero { color: var(--cms-color-text); }";

        var shared = CmsSharedPageAssets.Share(edited, truncated) ?? string.Empty;
        shared.ShouldBe(edited);
        shared.ShouldContain("@font-face");
        shared.ShouldContain("font-family: \"Vazirmatn\"");
        shared.ShouldContain("direction: rtl");
        shared.ShouldContain("margin-left: 1rem");
        shared.ShouldContain("margin-right: 2rem");
        shared.ShouldContain("left: 0");
        shared.ShouldContain("right: 4px");
        shared.ShouldContain("سلام");
        shared.ShouldContain("مرحبا");
        CmsSharedPageAssets.ChangesOtherCulture(edited, truncated).ShouldBeTrue();
        CmsSharedPageAssets.ChangesOtherCulture(edited, edited).ShouldBeFalse();
        CmsSharedPageAssets.Share(null, truncated).ShouldBeNull();
        CmsSharedPageAssets.Share("   ", truncated).ShouldBeNull();
    }
}
