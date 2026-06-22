namespace SufiChain.SufiAbp.AI.RAG;

public class IndexingStatusDto
{
    public string SourceName { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int IndexedDocuments { get; set; }
    public DateTime? LastIndexedAt { get; set; }
    public bool IsIndexing { get; set; }
    public int ProgressPercentage => TotalDocuments > 0 ? (IndexedDocuments * 100 / TotalDocuments) : 0;
}
