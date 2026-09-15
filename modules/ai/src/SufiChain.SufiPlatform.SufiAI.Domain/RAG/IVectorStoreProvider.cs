namespace SufiChain.SufiPlatform.SufiAI.RAG;

public interface IVectorStoreProvider
{
    VectorStoreType Type { get; }

    Task StoreEmbeddingsAsync(
        VectorStoreContext context,
        List<DocumentChunk> documents,
        CancellationToken cancellationToken = default
    );

    Task<List<DocumentChunk>> SearchSimilarAsync(
        VectorStoreContext context,
        float[] queryEmbedding,
        int maxResults = 10,
        float minSimilarity = 0.7f,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        VectorStoreContext context,
        string documentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes every chunk that originated from one source document, including split chunks
    /// whose <see cref="DocumentChunk.Id"/> differs from the source identifier.
    /// </summary>
    Task DeleteBySourceAsync(
        VectorStoreContext context,
        string sourceName,
        string sourceId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts chunks in the workspace scope. Honors <see cref="VectorStoreContext.SourceName"/>
    /// and <see cref="VectorStoreContext.MetadataFilters"/> when they are set.
    /// </summary>
    Task<int> GetCountAsync(
        VectorStoreContext context,
        CancellationToken cancellationToken = default
    );

    Task<IndexingStatus?> GetIndexingStatusAsync(
        VectorStoreContext context,
        string sourceName,
        CancellationToken cancellationToken = default
    );

    Task UpdateIndexingStatusAsync(
        VectorStoreContext context,
        IndexingStatus status,
        CancellationToken cancellationToken = default
    );
}

public class VectorStoreContext
{
    public string WorkspaceName { get; set; } = string.Empty;
    public VectorStoreType Type { get; set; }
    public string CollectionName { get; set; } = "ai_documents";
    public string? ConnectionString { get; set; }
    public string? ApiKey { get; set; }
    public int Dimensions { get; set; } = 1536;
    public int MaxInputTokens { get; set; } = EmbeddingModelDefaults.FallbackMaxInputTokens;
    public string? EmbedderFingerprint { get; set; }
    public Guid? TenantId { get; set; }
    public string TenantKey { get; set; } = "host";
    public string? Schema { get; set; }
    public string? TableName { get; set; }
    public string? ProviderName { get; set; }

    /// <summary>
    /// Optional source-name filter for similarity search.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Optional exact-match metadata filters for similarity search.
    /// </summary>
    public Dictionary<string, string> MetadataFilters { get; set; } = new();
}
