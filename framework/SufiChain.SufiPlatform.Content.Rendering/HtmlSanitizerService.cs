using Ganss.Xss;

namespace SufiChain.SufiPlatform.Content.Rendering;

public sealed class HtmlSanitizerService : IHtmlSanitizer
{
    private readonly Dictionary<string, HtmlSanitizer> _policies = new(StringComparer.OrdinalIgnoreCase)
    {
        [HtmlSanitizerPolicies.KnowledgeBasePublic] = CreatePublic(),
        [HtmlSanitizerPolicies.CmsPublic] = CreatePublic(),
        [HtmlSanitizerPolicies.Chat] = CreatePublic(),
        [HtmlSanitizerPolicies.Email] = CreateEmail()
    };

    public string Sanitize(string? html, string policyName)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        var sanitizer = _policies.TryGetValue(policyName, out var named) ? named : _policies[HtmlSanitizerPolicies.CmsPublic];
        return sanitizer.Sanitize(html);
    }

    private static HtmlSanitizer CreatePublic()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Remove("script");
        sanitizer.AllowedTags.Remove("iframe");
        sanitizer.AllowedTags.Remove("object");
        sanitizer.AllowedTags.Remove("embed");
        sanitizer.AllowedTags.Remove("form");
        sanitizer.AllowDataAttributes = true;
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("id");
        sanitizer.AllowedAttributes.Add("role");
        sanitizer.AllowedAttributes.Add("tabindex");
        sanitizer.AllowedAttributes.Add("aria-hidden");
        sanitizer.AllowedAttributes.Add("aria-label");
        sanitizer.AllowedAttributes.Add("aria-expanded");
        sanitizer.AllowedAttributes.Add("aria-controls");
        sanitizer.AllowedAttributes.Add("aria-pressed");
        sanitizer.AllowedAttributes.Add("aria-current");
        sanitizer.AllowedAttributes.Add("viewbox");
        sanitizer.AllowedAttributes.Add("xmlns");
        sanitizer.AllowedAttributes.Add("d");
        sanitizer.AllowedAttributes.Add("cx");
        sanitizer.AllowedAttributes.Add("cy");
        sanitizer.AllowedAttributes.Add("r");
        sanitizer.AllowedAttributes.Add("rx");
        sanitizer.AllowedAttributes.Add("ry");
        sanitizer.AllowedAttributes.Add("x");
        sanitizer.AllowedAttributes.Add("y");
        sanitizer.AllowedAttributes.Add("x1");
        sanitizer.AllowedAttributes.Add("y1");
        sanitizer.AllowedAttributes.Add("x2");
        sanitizer.AllowedAttributes.Add("y2");
        sanitizer.AllowedAttributes.Add("points");
        sanitizer.AllowedAttributes.Add("fill");
        sanitizer.AllowedAttributes.Add("stroke");
        sanitizer.AllowedAttributes.Add("transform");
        sanitizer.AllowedAttributes.Add("width");
        sanitizer.AllowedAttributes.Add("height");
        sanitizer.AllowedAttributes.Add("preserveaspectratio");
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("offset");
        sanitizer.AllowedAttributes.Add("stop-color");
        sanitizer.AllowedAttributes.Add("stop-opacity");
        sanitizer.AllowedAttributes.Add("fill-opacity");
        sanitizer.AllowedAttributes.Add("stroke-width");
        sanitizer.AllowedAttributes.Add("stroke-linecap");
        sanitizer.AllowedAttributes.Add("stroke-linejoin");
        sanitizer.AllowedAttributes.Add("stroke-dasharray");
        sanitizer.AllowedAttributes.Add("opacity");
        sanitizer.AllowedAttributes.Add("clip-path");
        sanitizer.AllowedAttributes.Add("gradientunits");
        sanitizer.AllowedTags.Add("section");
        sanitizer.AllowedTags.Add("article");
        sanitizer.AllowedTags.Add("figure");
        sanitizer.AllowedTags.Add("figcaption");
        sanitizer.AllowedTags.Add("svg");
        sanitizer.AllowedTags.Add("path");
        sanitizer.AllowedTags.Add("g");
        sanitizer.AllowedTags.Add("defs");
        sanitizer.AllowedTags.Add("lineargradient");
        sanitizer.AllowedTags.Add("radialgradient");
        sanitizer.AllowedTags.Add("stop");
        sanitizer.AllowedTags.Add("polygon");
        sanitizer.AllowedTags.Add("ellipse");
        sanitizer.AllowedTags.Add("circle");
        sanitizer.AllowedTags.Add("rect");
        sanitizer.AllowedTags.Add("symbol");
        sanitizer.AllowedTags.Add("use");
        sanitizer.AllowedTags.Add("line");
        sanitizer.AllowedTags.Add("polyline");
        sanitizer.AllowedTags.Add("clippath");
        sanitizer.AllowedTags.Add("mask");
        sanitizer.AllowedTags.Add("title");
        sanitizer.AllowedTags.Add("desc");
        sanitizer.AllowedTags.Add("main");
        sanitizer.AllowedTags.Add("nav");
        sanitizer.AllowedTags.Add("header");
        sanitizer.AllowedTags.Add("footer");
        sanitizer.AllowedTags.Add("aside");
        sanitizer.AllowedAttributes.Add("xlink:href");
        sanitizer.AllowedAttributes.Add("fill-rule");
        sanitizer.AllowedAttributes.Add("clip-rule");
        sanitizer.AllowedAttributes.Add("data-sb-callout");
        sanitizer.AllowedSchemes.Add("mailto");
        sanitizer.AllowedSchemes.Add("tel");
        return sanitizer;
    }

    private static HtmlSanitizer CreateEmail()
    {
        var sanitizer = CreatePublic();
        sanitizer.AllowedTags.Remove("svg");
        sanitizer.AllowedAttributes.Add("style");
        sanitizer.AllowedAttributes.Add("width");
        sanitizer.AllowedAttributes.Add("height");
        sanitizer.AllowedAttributes.Add("align");
        sanitizer.AllowedAttributes.Add("bgcolor");
        return sanitizer;
    }
}
