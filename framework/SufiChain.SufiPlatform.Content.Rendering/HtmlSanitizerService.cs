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
        sanitizer.AllowedAttributes.Add("class");
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
