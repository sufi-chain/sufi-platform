namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Runtime options attached to a platform copilot seed or definition.
/// </summary>
public class CopilotRuntimeOptions
{
    public float? Temperature { get; set; }

    public bool UseWebSearch { get; set; }
    public int? WebSearchMaxResults { get; set; }
    public int? WebSearchMaxPagesToFetch { get; set; }

    public bool UseRag { get; set; }

    public int? RagTopK { get; set; }

    /// <summary>
    /// Question-to-passage similarity floor. Null uses
    /// <c>CopilotRagRuntimeOptionsDefaults.SearchMinSimilarity</c>.
    /// </summary>
    public float? RagMinSimilarity { get; set; }

    /// <summary>
    /// Floor used when expanding sibling chunks of a hit article. Null uses
    /// <c>CopilotRagRuntimeOptionsDefaults.ArticleExpandMinSimilarity</c>.
    /// </summary>
    public float? RagArticleExpandMinSimilarity { get; set; }

    /// <summary>
    /// Cap for expanded article chunks. Null uses
    /// <c>CopilotRagRuntimeOptionsDefaults.ArticleExpandMaxResults</c>.
    /// </summary>
    public int? RagArticleExpandMaxResults { get; set; }

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

    /// <summary>Chat composer emoji picker. Default on to match historical visitor and copilot UI.</summary>
    public bool AllowComposerEmoji { get; set; } = true;

    /// <summary>Chat composer voice recording.</summary>
    public bool AllowComposerAudio { get; set; }

    /// <summary>Chat composer file attachments.</summary>
    public bool AllowComposerAttachment { get; set; }

    /// <summary>Chat composer context-usage toolbar.</summary>
    public bool AllowComposerContext { get; set; }

    public CopilotComposerToolbarOptions GetComposerToolbar(bool allowUserModelSelection) =>
        CopilotComposerToolbarOptions.FromRuntime(this, allowUserModelSelection);
}
