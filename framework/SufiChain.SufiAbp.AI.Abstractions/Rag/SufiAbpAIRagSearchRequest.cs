namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Request for a semantic RAG search against a named AI workspace.
/// </summary>
public class SufiAbpAIRagSearchRequest
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
}
