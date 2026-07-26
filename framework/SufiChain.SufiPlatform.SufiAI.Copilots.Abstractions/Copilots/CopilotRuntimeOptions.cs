namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Runtime options attached to a platform copilot seed or definition.
/// </summary>
public class CopilotRuntimeOptions
{
    public float? Temperature { get; set; }

    public bool UseRag { get; set; }

    public int? RagTopK { get; set; }

    /// <summary>
    /// Optional document source name filter applied when <see cref="UseRag"/> is true
    /// (for example <c>KnowledgeBase</c>).
    /// </summary>
    public string? RagSourceName { get; set; }

    /// <summary>
    /// Optional static metadata filters for RAG search. Per-turn context keys such as
    /// <c>projectId</c> are merged by the runtime when present on the request.
    /// </summary>
    public Dictionary<string, string> RagMetadataFilters { get; set; } = new();

    /// <summary>
    /// When true, merges request <c>copilotContext.projectId</c> into RAG metadata filters.
    /// </summary>
    public bool RagFilterByProjectId { get; set; }

    public bool UseMcpTools { get; set; }

    public List<string> AllowedMcpToolNames { get; set; } = new();
}
