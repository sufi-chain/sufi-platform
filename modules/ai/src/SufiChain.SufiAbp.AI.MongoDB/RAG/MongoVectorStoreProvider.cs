using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiAbp.AI.RAG;

namespace SufiChain.SufiAbp.AI.MongoDB.RAG;

public class MongoVectorStoreProvider : IVectorStoreProvider, ITransientDependency
{
    private readonly IMongoClient _mongoClient;
    
    public VectorStoreType Type => VectorStoreType.MongoDB;

    public MongoVectorStoreProvider(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task StoreEmbeddingsAsync(
        string collectionName,
        List<DocumentChunk> documents,
        CancellationToken cancellationToken = default)
    {
        var database = _mongoClient.GetDatabase("AI");
        var collection = database.GetCollection<BsonDocument>(collectionName);

        var bsonDocuments = documents.Select(doc => new BsonDocument
        {
            { "documentId", doc.Id },
            { "sourceName", doc.SourceName },
            { "sourceId", doc.SourceId },
            { "content", doc.Content },
            { "metadata", BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(doc.Metadata)) },
            { "embedding", new BsonArray(doc.Embedding) },
            { "createdAt", doc.CreatedAt },
            { "updatedAt", doc.UpdatedAt }
        }).ToList();

        if (bsonDocuments.Any())
        {
            await collection.InsertManyAsync(bsonDocuments, cancellationToken: cancellationToken);
        }
    }

    public async Task<List<DocumentChunk>> SearchSimilarAsync(
        string collectionName,
        float[] queryEmbedding,
        int maxResults = 10,
        float minSimilarity = 0.7f,
        CancellationToken cancellationToken = default)
    {
        var database = _mongoClient.GetDatabase("AI");
        var collection = database.GetCollection<BsonDocument>(collectionName);

        var allDocs = await collection.Find(new BsonDocument()).ToListAsync(cancellationToken);
        
        var results = allDocs
            .Select(doc => new
            {
                Document = doc,
                Similarity = CosineSimilarity(
                    queryEmbedding,
                    doc["embedding"].AsBsonArray.Select(v => (float)v.AsDouble).ToArray()
                )
            })
            .Where(x => x.Similarity >= minSimilarity)
            .OrderByDescending(x => x.Similarity)
            .Take(maxResults)
            .Select(x => new DocumentChunk
            {
                Id = x.Document["documentId"].AsString,
                SourceName = x.Document["sourceName"].AsString,
                SourceId = x.Document["sourceId"].AsString,
                Content = x.Document["content"].AsString,
                Metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    x.Document["metadata"].ToJson()) ?? new(),
                Embedding = x.Document["embedding"].AsBsonArray.Select(v => (float)v.AsDouble).ToArray(),
                CreatedAt = x.Document["createdAt"].ToUniversalTime()
            })
            .ToList();

        return results;
    }

    public async Task DeleteAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var database = _mongoClient.GetDatabase("AI");
        var collection = database.GetCollection<BsonDocument>(collectionName);

        await collection.DeleteOneAsync(
            Builders<BsonDocument>.Filter.Eq("documentId", documentId),
            cancellationToken
        );
    }

    public async Task<int> GetCountAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var database = _mongoClient.GetDatabase("AI");
        var collection = database.GetCollection<BsonDocument>(collectionName);

        return (int)await collection.CountDocumentsAsync(new BsonDocument(), cancellationToken: cancellationToken);
    }

    private float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            return 0;

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        return dotProduct / (MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB));
    }
}
