namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Request to (re)index one source document into a workspace's vector store.
/// The document is loaded through <see cref="ISufiAIDocumentSource.GetByIdAsync"/>;
/// when the source no longer exposes it, previously stored chunks are removed.
/// </summary>
public class SufiAIRagIndexDocumentRequest
{
    /// <summary>
    /// Name of the AI workspace whose vector store is updated.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Document source that owns the document (for example <c>KnowledgeBase</c>).
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Source document identifier as understood by <see cref="ISufiAIDocumentSource.GetByIdAsync"/>.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;
}

/// <summary>
/// Outcome of <see cref="ISufiAIRagService.IndexDocumentAsync"/>.
/// </summary>
public class SufiAIRagIndexDocumentResult
{
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Number of chunks written to the vector store for this document in this call.
    /// Zero means the document was not indexable (for example unpublished or filtered out).
    /// </summary>
    public int StoredChunkCount { get; set; }

    /// <summary>
    /// True when stale chunks for the document were removed because the source no longer exposes it.
    /// </summary>
    public bool Removed { get; set; }

    public bool IsIndexed => StoredChunkCount > 0;
}
