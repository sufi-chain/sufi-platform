using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.RAG;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAbpAIRagService))]
public class SufiAbpAIRagServiceAdapter : ISufiAbpAIRagService, ITransientDependency
{
    protected IRAGService RagService { get; }

    public SufiAbpAIRagServiceAdapter(IRAGService ragService)
    {
        RagService = ragService;
    }

    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public virtual async Task<SufiAbpAIRagSearchResult> SearchAsync(
        SufiAbpAIRagSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var chunks = await RagService.SearchAsync(
            request.WorkspaceName,
            request.Query,
            request.MaxResults,
            cancellationToken);

        return new SufiAbpAIRagSearchResult
        {
            Chunks = chunks.Select(MapChunk).ToList()
        };
    }

    public virtual async Task IndexAsync(
        SufiAbpAIRagIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SourceName))
        {
            await RagService.IndexAllDocumentsAsync(request.WorkspaceName, cancellationToken: cancellationToken);
            return;
        }

        await RagService.IndexDocumentsAsync(request.WorkspaceName, request.SourceName, cancellationToken: cancellationToken);
    }

    public virtual async Task<SufiAbpAIIndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        var status = await RagService.GetIndexingStatusAsync(workspaceName, sourceName, cancellationToken);

        return new SufiAbpAIIndexingStatus
        {
            SourceName = status.SourceName,
            TotalDocuments = status.TotalDocuments,
            IndexedDocuments = status.IndexedDocuments,
            LastIndexedAt = status.LastIndexedAt,
            IsIndexing = status.IsIndexing
        };
    }

    public static SufiAbpAIDocumentChunk MapChunk(DocumentChunk chunk)
    {
        return new SufiAbpAIDocumentChunk
        {
            Id = chunk.Id,
            SourceName = chunk.SourceName,
            SourceId = chunk.SourceId,
            Content = chunk.Content,
            Metadata = chunk.Metadata,
            Embedding = chunk.Embedding,
            CreatedAt = chunk.CreatedAt,
            UpdatedAt = chunk.UpdatedAt
        };
    }

    public static DocumentChunk MapChunk(SufiAbpAIDocumentChunk chunk)
    {
        return new DocumentChunk
        {
            Id = chunk.Id,
            SourceName = chunk.SourceName,
            SourceId = chunk.SourceId,
            Content = chunk.Content,
            Metadata = chunk.Metadata,
            Embedding = chunk.Embedding,
            CreatedAt = chunk.CreatedAt,
            UpdatedAt = chunk.UpdatedAt
        };
    }
}
