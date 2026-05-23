namespace SufiChain.SufiAbp.AIManagement.RAG;

public interface IDocumentSource
{
    string SourceName { get; }
    
    Task<List<DocumentChunk>> SearchAsync(
        string? query = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default
    );
    
    Task<DocumentChunk?> GetByIdAsync(
        string documentId,
        CancellationToken cancellationToken = default
    );
    
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
