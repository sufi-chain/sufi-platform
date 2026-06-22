namespace SufiChain.SufiAbp.AI.RAG;

public interface IVectorStoreProvider
{
    VectorStoreType Type { get; }
    
    Task StoreEmbeddingsAsync(
        string collectionName,
        List<DocumentChunk> documents,
        CancellationToken cancellationToken = default
    );
    
    Task<List<DocumentChunk>> SearchSimilarAsync(
        string collectionName,
        float[] queryEmbedding,
        int maxResults = 10,
        float minSimilarity = 0.7f,
        CancellationToken cancellationToken = default
    );
    
    Task DeleteAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default
    );
    
    Task<int> GetCountAsync(
        string collectionName,
        CancellationToken cancellationToken = default
    );
}
