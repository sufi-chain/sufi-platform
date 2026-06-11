using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// A document chunk used for RAG indexing and returned from RAG search.
/// Replaces both the provider domain chunk and search-result DTO shapes.
/// </summary>
public class SufiAbpAIDocumentChunk
{
    /// <summary>
    /// Unique chunk identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the document source that produced the chunk.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the originating document within the source.
    /// </summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// Chunk text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Arbitrary metadata attached to the chunk (e.g. title, URL, tenant, project).
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Relevance score for search results; 0 when not applicable (e.g. during indexing).
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Vector embedding, populated by the indexing pipeline when available.
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Creation timestamp of the originating document.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp of the originating document, when known.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
