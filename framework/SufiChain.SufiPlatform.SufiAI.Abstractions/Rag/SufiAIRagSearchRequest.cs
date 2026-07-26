namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Request for a semantic RAG search against a named AI workspace.
/// </summary>
public class SufiAIRagSearchRequest
{
    /// <summary>
    /// Name of the AI workspace whose vector store is searched.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Search query text.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of chunks to return.
    /// </summary>
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// When set, only chunks whose document source name matches are returned.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Exact-match filters against document metadata keys (for example <c>projectId</c>).
    /// Applied at search time when the vector store supports them; callers should still
    /// treat results as scoped to the requested tenant workspace.
    /// </summary>
    public Dictionary<string, string> MetadataFilters { get; set; } = new();
}
