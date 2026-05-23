namespace SufiChain.SufiAbp.AIManagement.RAG;

public interface IRAGService
{
    void RegisterDocumentSource(IDocumentSource source);
    List<IDocumentSource> GetDocumentSources();
    
    Task<List<DocumentChunk>> SearchAsync(
        string workspaceName,
        string query,
        int maxResults = 10,
        CancellationToken cancellationToken = default
    );
    
    Task IndexDocumentsAsync(
        string workspaceName,
        string sourceName,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
    
    Task IndexAllDocumentsAsync(
        string workspaceName,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
    
    Task<IndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default
    );
}

public class IndexingProgress
{
    public string SourceName { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int IndexedDocuments { get; set; }
    public int FailedDocuments { get; set; }
    public string? CurrentDocument { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class IndexingStatus
{
    public string SourceName { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int IndexedDocuments { get; set; }
    public DateTime? LastIndexedAt { get; set; }
    public bool IsIndexing { get; set; }
}
