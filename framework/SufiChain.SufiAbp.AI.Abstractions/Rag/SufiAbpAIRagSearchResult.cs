using System.Collections.Generic;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Result of a semantic RAG search.
/// </summary>
public class SufiAbpAIRagSearchResult
{
    /// <summary>
    /// Matching chunks ordered by descending relevance.
    /// </summary>
    public List<SufiAbpAIDocumentChunk> Chunks { get; set; } = new();
}
