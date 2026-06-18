using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.RAG;
using Volo.Abp;

namespace SufiChain.SufiAbp.AIManagement.Controllers;

[Area(AIManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai-management/rag")]
public class RAGController : AIManagementController, IRAGAppService
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
