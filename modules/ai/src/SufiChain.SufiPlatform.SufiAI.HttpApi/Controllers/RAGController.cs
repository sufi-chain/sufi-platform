using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/rag")]
public class RAGController : AIController, IRAGAppService
{
    private readonly IRAGAppService _ragAppService;

    public RAGController(IRAGAppService ragAppService)
    {
        _ragAppService = ragAppService;
    }

    [HttpGet("document-sources")]
    public virtual Task<List<DocumentSourceDto>> GetDocumentSourcesAsync()
    {
        return _ragAppService.GetDocumentSourcesAsync();
    }

    [HttpGet("availability")]
    public virtual Task<RagAvailabilityDto> GetAvailabilityAsync()
    {
        return _ragAppService.GetAvailabilityAsync();
    }

    [HttpPost("documents/search")]
    public virtual Task<List<DocumentChunkDto>> SearchDocumentsAsync(SearchDocumentsInput input)
    {
        return _ragAppService.SearchDocumentsAsync(input);
    }

    [HttpGet("workspaces/{workspaceName}/sources/{sourceName}/indexing-status")]
    public virtual Task<IndexingStatusDto> GetIndexingStatusAsync(string workspaceName, string sourceName)
    {
        return _ragAppService.GetIndexingStatusAsync(workspaceName, sourceName);
    }

    [HttpPost("workspaces/{workspaceName}/sources/{sourceName}/start-indexing")]
    public virtual Task StartIndexingAsync(string workspaceName, string sourceName)
    {
        return _ragAppService.StartIndexingAsync(workspaceName, sourceName);
    }
}
