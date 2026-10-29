using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Html;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

/// <summary>sufi-chain#340: a contact section is stored with a real form block bound to a published form key.</summary>
public class CmsFormBlockBinderTests
{
    [Fact]
    public void An_alias_attribute_is_rewritten_to_form()
    {
        var html = """<section><cms-block name="form" formKey="qaag_workshop_registration"></cms-block></section>""";
        var bound = CmsFormBlockBinder.Bind(html, new[] { "qaag_workshop_registration" });

        bound.ShouldContain("form=\"qaag_workshop_registration\"");
        bound.ShouldNotContain("formKey=");
    }

    [Fact]
    public void A_mentioned_key_inserts_a_block_when_none_was_written()
    {
        var html = "<section><h2>Contact</h2><p>Use qaag_workshop_registration.</p></section>";
        var bound = CmsFormBlockBinder.Bind(html, new[] { "qaag_workshop_registration", "other_form" });

        bound.ShouldContain("<cms-block name=\"form\" form=\"qaag_workshop_registration\"></cms-block>");
        bound.ShouldContain("</section>");
        bound.IndexOf("cms-block", StringComparison.Ordinal).ShouldBeLessThan(bound.IndexOf("</section>", StringComparison.Ordinal));
    }

    [Fact]
    public void A_single_published_form_is_bound_into_a_contact_heading()
    {
        var html = "<section><h2>تماس با ما</h2><p>پیام بگذارید.</p></section>";
        var bound = CmsFormBlockBinder.Bind(html, new[] { "contact_form" });

        bound.ShouldContain("form=\"contact_form\"");
    }

    [Fact]
    public void Html_is_unchanged_when_no_form_is_published()
    {
        var html = "<section><h2>Contact</h2></section>";
        CmsFormBlockBinder.Bind(html, Array.Empty<string>()).ShouldBe(html);
    }

    [Fact]
    public void The_form_attribute_survives_html_parse_and_serialize()
    {
        var html = """<section><cms-block name="form" form="qaag_workshop_registration"></cms-block></section>""";
        var serialized = CmsHtml.Serialize(CmsHtml.Parse(html));

        serialized.ShouldContain("form=\"qaag_workshop_registration\"");
    }
}
