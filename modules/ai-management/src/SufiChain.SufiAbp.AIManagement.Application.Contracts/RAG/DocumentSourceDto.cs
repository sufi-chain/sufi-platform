namespace SufiChain.SufiAbp.AIManagement.RAG;

public class DocumentSourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public int TotalDocuments { get; set; } // Backward compatibility
    public DateTime? LastIndexedAt { get; set; }
    public IndexingStatusType Status { get; set; }
    public int? Progress { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum IndexingStatusType { Pending, Indexing, Complete, Failed }
