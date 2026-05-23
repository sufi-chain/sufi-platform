using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AIManagement.RAG;

public interface IRAGAppService : IApplicationService
{
    Task<List<DocumentSourceDto>> GetDocumentSourcesAsync();
    
    Task<List<DocumentChunkDto>> SearchDocumentsAsync(SearchDocumentsInput input);
    
    Task<IndexingStatusDto> GetIndexingStatusAsync(string workspaceName, string sourceName);
    
    Task StartIndexingAsync(string workspaceName, string sourceName);
}
