using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.RAG;

public interface IRAGAppService : IApplicationService
{
    Task<RagAvailabilityDto> GetAvailabilityAsync();
    Task<List<DocumentSourceDto>> GetDocumentSourcesAsync();
    
    Task<List<DocumentChunkDto>> SearchDocumentsAsync(SearchDocumentsInput input);
    
    Task<IndexingStatusDto> GetIndexingStatusAsync(string workspaceName, string sourceName);
    
    Task StartIndexingAsync(string workspaceName, string sourceName);
}
