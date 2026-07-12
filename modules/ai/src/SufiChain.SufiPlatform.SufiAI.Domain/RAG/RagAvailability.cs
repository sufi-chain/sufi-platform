namespace SufiChain.SufiPlatform.SufiAI.RAG;

public class RagAvailability
{
    public bool IsAvailable { get; set; }
    public RagProviderKind Provider { get; set; }
    public string? Message { get; set; }
}
