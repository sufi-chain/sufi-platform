using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Request to count stored chunks in a workspace's vector store using the same
/// source and metadata filters that <see cref="SufiAIRagSearchRequest"/> applies at search time.
/// Used to prove that vectors for a document actually exist before reporting it as indexed.
/// </summary>
public class SufiAIRagCountRequest
{
    /// <summary>
    /// Name of the AI workspace whose vector store is counted.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// When set, only chunks whose document source name matches are counted.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Exact-match filters against document metadata keys (for example <c>articleId</c>).
    /// </summary>
    public Dictionary<string, string> MetadataFilters { get; set; } = new();
}
