using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Platform-level RAG (retrieval-augmented generation) service for product modules,
/// covering both indexing and semantic search. Stable public contract; a provider
/// module (e.g. AI) replaces the Null default. Vector-store and admin
/// details remain in the provider layer.
/// </summary>
public interface ISufiAIRagService
{
    /// <summary>
    /// Whether a real RAG provider implementation is installed and usable.
    /// Returns <c>false</c> for the Null fallback so modules can degrade gracefully.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a semantic search over the workspace's indexed documents.
    /// </summary>
    Task<SufiAIRagSearchResult> SearchAsync(
        SufiAIRagSearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// (Re)indexes documents from the requested source (or all sources) into the
    /// workspace's vector store. Sources are discovered from registered
    /// <see cref="ISufiAIDocumentSource"/> implementations.
    /// </summary>
    Task IndexAsync(
        SufiAIRagIndexRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// (Re)indexes a single source document by identifier. Removes stale chunks when the
    /// source no longer exposes the document. Returns how many chunks were stored so callers
    /// can report an indexed state only when vectors exist.
    /// </summary>
    Task<SufiAIRagIndexDocumentResult> IndexDocumentAsync(
        SufiAIRagIndexDocumentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts stored chunks that match the workspace, source, and metadata filters.
    /// Returns <c>0</c> for the Null fallback.
    /// </summary>
    Task<int> CountAsync(
        SufiAIRagCountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the indexing status of a document source within a workspace.
    /// </summary>
    Task<SufiAIIndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default);
}
