using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.RAG;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

public class SufiAbpAIDocumentSourceAdapter : IDocumentSource
{
    protected ISufiAbpAIDocumentSource InnerSource { get; }

    public SufiAbpAIDocumentSourceAdapter(ISufiAbpAIDocumentSource innerSource)
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
        return chunks.Select(SufiAbpAIRagServiceAdapter.MapChunk).ToList();
    }

    public virtual async Task<DocumentChunk?> GetByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var chunk = await InnerSource.GetByIdAsync(documentId, cancellationToken);
        return chunk == null ? null : SufiAbpAIRagServiceAdapter.MapChunk(chunk);
    }

    public virtual Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return InnerSource.GetTotalCountAsync(cancellationToken);
    }
}
