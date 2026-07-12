namespace SufiChain.SufiPlatform.SufiAI.RAG;

public class DocumentSourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public DateTime? LastIndexedAt { get; set; }
    public IndexingStatusType Status { get; set; }
    public int? Progress { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum IndexingStatusType { Pending, Indexing, Complete, Failed }
