namespace SufiChain.SufiAbp.AI.RAG;

public class DocumentChunk
{
    public string Id { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public float[]? Embedding { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
