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
    public const string Email = "Email";
    public const string Chat = "Chat";
}
