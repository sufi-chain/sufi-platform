namespace SufiChain.SufiPlatform.Content.Rendering;

public interface IMarkdownRenderer
{
    string ToHtml(string? markdown, MarkdownRenderOptions? options = null);
}

public sealed class MarkdownRenderOptions
{
    public bool AllowRawHtml { get; set; }
}

public interface IHtmlSanitizer
{
    string Sanitize(string? html, string policyName);
}

public static class HtmlSanitizerPolicies
{
    public const string KnowledgeBasePublic = "KnowledgeBasePublic";
    public const string CmsPublic = "CmsPublic";

    /// <summary>
    /// Editor-authored CMS page markup: layout tags, inline styles, media and https embeds; never scripts,
    /// event handlers or forms (forms come from the <c>form</c> block).
    /// </summary>
    public const string CmsPage = "CmsPage";
    public const string Email = "Email";
    public const string Chat = "Chat";
}
