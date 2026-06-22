using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// A pluggable document source that feeds the RAG indexing pipeline.
/// Product modules implement this interface (registered as a transient service);
/// the RAG provider discovers all registered sources automatically.
/// </summary>
public interface ISufiAIDocumentSource
{
    /// <summary>
    /// Unique source name (e.g. <c>KnowledgeBase</c>).
    /// </summary>
    string SourceName { get; }

    /// <summary>
    /// Lists chunks from the source, optionally filtered by a text query.
    /// </summary>
    Task<List<SufiAIDocumentChunk>> SearchAsync(
        string? query = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a chunk by its document identifier, or <c>null</c> when not found.
    /// </summary>
    Task<SufiAIDocumentChunk?> GetByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total number of indexable documents in the source.
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
