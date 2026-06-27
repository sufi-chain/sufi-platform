namespace SufiChain.SufiAbp.AI.RAG;

public class DocumentChunkDto
{
    public string Id { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public float Score { get; set; }
}
