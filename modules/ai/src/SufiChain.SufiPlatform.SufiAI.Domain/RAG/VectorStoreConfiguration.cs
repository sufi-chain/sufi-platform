namespace SufiChain.SufiPlatform.SufiAI.RAG;

/// <summary>
/// Runtime embedder settings resolved from multimodal <c>AIModelConfiguration</c> (Embeddings).
/// </summary>
public class EmbedderConfiguration
{
    public AIProviderType Provider { get; set; }
    public string Model { get; set; } = string.Empty;
   public string? ApiKey { get; set; }
   public string? ApiBaseUrl { get; set; }
    public int Dimensions { get; set; } = EmbeddingModelDefaults.FallbackDimensions;
}

public class VectorStoreConfiguration
{
    public VectorStoreType Type { get; set; }
    public string? ConnectionString { get; set; }
    public string? ApiKey { get; set; }
    public string CollectionName { get; set; } = "ai_documents";
    public int Dimensions { get; set; } = 1536;
    public string? Schema { get; set; }
    public string? TableName { get; set; }
    public string? ProviderName { get; set; }
}
