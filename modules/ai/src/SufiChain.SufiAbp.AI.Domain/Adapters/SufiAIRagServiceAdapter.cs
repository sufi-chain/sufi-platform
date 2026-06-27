using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.RAG;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIRagService))]
public class SufiAIRagServiceAdapter : ISufiAIRagService, ITransientDependency
{
    protected IRAGService RagService { get; }

    public SufiAIRagServiceAdapter(IRAGService ragService)
    {
        RagService = ragService;
    }

    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return IsAvailableInternalAsync(cancellationToken);
    }

    public virtual async Task<SufiAIRagSearchResult> SearchAsync(
        SufiAIRagSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var chunks = await RagService.SearchAsync(
            request.WorkspaceName,
            request.Query,
            request.MaxResults,
            cancellationToken: cancellationToken);

        return new SufiAIRagSearchResult
        {
            Chunks = chunks.Select(MapChunk).ToList()
        };
    }

    public virtual async Task IndexAsync(
        SufiAIRagIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.SourceName))
        {
            await RagService.IndexAllDocumentsAsync(request.WorkspaceName, cancellationToken: cancellationToken);
            return;
        }

        await RagService.IndexDocumentsAsync(request.WorkspaceName, request.SourceName, cancellationToken: cancellationToken);
    }

    public virtual async Task<SufiAIIndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        var status = await RagService.GetIndexingStatusAsync(workspaceName, sourceName, cancellationToken);

        return new SufiAIIndexingStatus
        {
            SourceName = status.SourceName,
            TotalDocuments = status.TotalDocuments,
            IndexedDocuments = status.IndexedDocuments,
            LastIndexedAt = status.LastIndexedAt,
            IsIndexing = status.IsIndexing
        };
    }

    public static SufiAIDocumentChunk MapChunk(DocumentChunk chunk)
    {
        return new SufiAIDocumentChunk
        {
            Id = chunk.Id,
            Score = chunk.Score,
            SourceName = chunk.SourceName,
            SourceId = chunk.SourceId,
            Content = chunk.Content,
            Metadata = chunk.Metadata,
            Embedding = chunk.Embedding,
            CreatedAt = chunk.CreatedAt,
            UpdatedAt = chunk.UpdatedAt
        };
    }

    public static DocumentChunk MapChunk(SufiAIDocumentChunk chunk)
    {
        return new DocumentChunk
        {
            Id = chunk.Id,
            Score = chunk.Score,
            SourceName = chunk.SourceName,
            SourceId = chunk.SourceId,
            Content = chunk.Content,
            Metadata = chunk.Metadata,
            Embedding = chunk.Embedding,
            CreatedAt = chunk.CreatedAt,
            UpdatedAt = chunk.UpdatedAt
        };
    }

    protected virtual async Task<bool> IsAvailableInternalAsync(CancellationToken cancellationToken)
    {
        var availability = await RagService.GetAvailabilityAsync(cancellationToken);
        return availability.IsAvailable;
    }
}
