using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Result of a semantic RAG search.
/// </summary>
public class SufiAIRagSearchResult
{
    /// <summary>
    /// Matching chunks ordered by descending relevance.
    /// </summary>
    public List<SufiAIDocumentChunk> Chunks { get; set; } = new();
}
