using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.RAG;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

public class SufiAIDocumentSourceAdapter : IDocumentSource
{
    protected ISufiAIDocumentSource InnerSource { get; }

    public SufiAIDocumentSourceAdapter(ISufiAIDocumentSource innerSource)
    {
        InnerSource = innerSource;
    }

    public string SourceName => InnerSource.SourceName;

    public virtual async Task<List<DocumentChunk>> SearchAsync(
        string? query = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var chunks = await InnerSource.SearchAsync(query, maxResults, cancellationToken);
        return chunks.Select(SufiAIRagServiceAdapter.MapChunk).ToList();
    }

    public virtual async Task<DocumentChunk?> GetByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var chunk = await InnerSource.GetByIdAsync(documentId, cancellationToken);
        return chunk == null ? null : SufiAIRagServiceAdapter.MapChunk(chunk);
    }

    public virtual Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return InnerSource.GetTotalCountAsync(cancellationToken);
    }
}
