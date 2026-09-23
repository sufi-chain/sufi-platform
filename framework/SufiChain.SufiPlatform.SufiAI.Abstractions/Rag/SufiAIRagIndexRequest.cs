namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Request to (re)index documents into a workspace's vector store.
/// </summary>
public class SufiAIRagIndexRequest
{
    /// <summary>
    /// Name of the AI workspace whose vector store is updated.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Document source to index. When <c>null</c>, all registered sources are indexed.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Optional exact-match metadata filters applied after the document source harvest
    /// (for example <c>projectId</c> for <c>HelpDesk.KnowledgeBase</c>, <c>hooshvareId</c> for <c>Hooshvare.KnowledgeBase</c>).
    /// </summary>
    public Dictionary<string, string> MetadataFilters { get; set; } = new();
}
