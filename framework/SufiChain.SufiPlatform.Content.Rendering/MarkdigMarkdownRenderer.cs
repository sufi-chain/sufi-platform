using System.Text.RegularExpressions;
using Markdig;

namespace SufiChain.SufiPlatform.Content.Rendering;

public sealed class MarkdigMarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline SafePipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private static readonly MarkdownPipeline HtmlPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static readonly Regex CalloutFence = new(
        @"^:::(\w+)\s*\n([\s\S]*?)\n:::\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public string ToHtml(string? markdown, MarkdownRenderOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var source = ExpandCallouts(markdown);
        var pipeline = options?.AllowRawHtml == true ? HtmlPipeline : SafePipeline;
        return Markdown.ToHtml(source, pipeline).Trim();
    }

    private static string ExpandCallouts(string markdown)
    {
        return CalloutFence.Replace(markdown, match =>
        {
            var kind = NormalizeKind(match.Groups[1].Value);
            var body = match.Groups[2].Value.Trim();
            return $"<div class=\"sb-callout sb-callout--{kind}\" data-sb-callout=\"{kind}\">\n\n{body}\n\n</div>\n";
        });
    }

    private static string NormalizeKind(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "tip" => "tip",
            "warn" or "warning" => "warn",
            "alert" or "danger" or "att" => "alert",
            _ => "note"
        };
    }
}
