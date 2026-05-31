using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.AI.Features;
using Volo.Abp.Application.Services;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.AIManagement.Permissions;

namespace SufiChain.SufiAbp.AIManagement.RAG;

[RequiresFeature(SufiAbpAIFeatures.Enable)]
[Authorize(AIManagementPermissions.RAG.Default)]
public class RAGAppService : ApplicationService, IRAGAppService
{
    private readonly IRAGService _ragService;

    public RAGAppService(IRAGService ragService)
    {
        _ragService = ragService;
    }

    public async Task<List<DocumentSourceDto>> GetDocumentSourcesAsync()
    {
        var sources = _ragService.GetDocumentSources();
        
        var result = new List<DocumentSourceDto>();
        foreach (var source in sources)
        {
            var count = await source.GetTotalCountAsync();
            result.Add(new DocumentSourceDto
            {
                SourceName = source.SourceName,
                TotalDocuments = count,
                LastIndexedAt = null // Will be tracked in future
            });
        }
        
        return result;
    }

    public async Task<List<DocumentChunkDto>> SearchDocumentsAsync(SearchDocumentsInput input)
    {
        var results = await _ragService.SearchAsync(
            input.WorkspaceName,
            input.Query,
            input.MaxResults
        );

        return ObjectMapper.Map<List<DocumentChunk>, List<DocumentChunkDto>>(results);
    }

    public async Task<IndexingStatusDto> GetIndexingStatusAsync(string workspaceName, string sourceName)
    {
        var status = await _ragService.GetIndexingStatusAsync(workspaceName, sourceName);
        return ObjectMapper.Map<IndexingStatus, IndexingStatusDto>(status);
    }

    [Authorize(AIManagementPermissions.RAG.Index)]
    public async Task StartIndexingAsync(string workspaceName, string sourceName)
    {
        await _ragService.IndexDocumentsAsync(workspaceName, sourceName);
    }
}
