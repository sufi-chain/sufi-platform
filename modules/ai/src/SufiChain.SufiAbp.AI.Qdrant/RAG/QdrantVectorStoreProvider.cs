using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SufiChain.SufiAbp.AI.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.RAG;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using static Qdrant.Client.Grpc.Conditions;

namespace SufiChain.SufiAbp.AI.Qdrant;

public class QdrantVectorStoreProvider : IVectorStoreProvider, ITransientDependency
{
    private readonly IDbContextProvider<AIDbContext> _dbContextProvider;

    public VectorStoreType Type => VectorStoreType.Qdrant;

    public QdrantVectorStoreProvider(IDbContextProvider<AIDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task StoreEmbeddingsAsync(VectorStoreContext context, List<DocumentChunk> documents, CancellationToken cancellationToken = default)
    {
        ValidateVectors(context, documents.Select(document => document.Embedding).Where(embedding => embedding != null).Cast<float[]>());

        var client = CreateClient(context);
        await EnsureCollectionExistsAsync(client, context, cancellationToken);

        var points = documents.Select(document => new PointStruct
        {
            Id = GetPointId(context.TenantKey, context.WorkspaceName, document.SourceName, document.SourceId, document.Id),
            Vectors = document.Embedding ?? Array.Empty<float>(),
            Payload =
            {
                ["tenantKey"] = context.TenantKey,
                ["tenantId"] = context.TenantId?.ToString() ?? string.Empty,
                ["workspaceName"] = context.WorkspaceName,
                ["sourceName"] = document.SourceName,
                ["sourceId"] = document.SourceId,
                ["documentId"] = document.Id,
                ["content"] = document.Content,
                ["metadataJson"] = JsonSerializer.Serialize(document.Metadata),
                ["createdAt"] = document.CreatedAt.ToString("O"),
                ["updatedAt"] = document.UpdatedAt?.ToString("O") ?? string.Empty
            }
        }).ToList();

        await client.UpsertAsync(context.CollectionName, points, cancellationToken: cancellationToken);
    }

    public async Task<List<DocumentChunk>> SearchSimilarAsync(VectorStoreContext context, float[] queryEmbedding, int maxResults = 10, float minSimilarity = 0.7f, CancellationToken cancellationToken = default)
    {
        ValidateVector(context, queryEmbedding);

        var client = CreateClient(context);
        await EnsureCollectionExistsAsync(client, context, cancellationToken);

        var results = await client.SearchAsync(
            collectionName: context.CollectionName,
            vector: queryEmbedding,
            filter: BuildWorkspaceFilter(context),
            limit: (ulong)Math.Max(1, maxResults),
            scoreThreshold: minSimilarity,
            cancellationToken: cancellationToken);

        return results
            .Select(result => ToDocumentChunk(context, result))
            .Where(document => document != null)
            .Cast<DocumentChunk>()
            .ToList();
    }

    public async Task DeleteAsync(VectorStoreContext context, string documentId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(context);
        await EnsureCollectionExistsAsync(client, context, cancellationToken);
        await client.DeleteAsync(context.CollectionName, BuildDocumentFilter(context, documentId), cancellationToken: cancellationToken);
    }

    public async Task<int> GetCountAsync(VectorStoreContext context, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(context);
        await EnsureCollectionExistsAsync(client, context, cancellationToken);
        var count = await client.CountAsync(context.CollectionName, BuildWorkspaceFilter(context), exact: true, cancellationToken: cancellationToken);
        return checked((int)count);
    }

    public async Task<IndexingStatus?> GetIndexingStatusAsync(VectorStoreContext context, string sourceName, CancellationToken cancellationToken = default)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var state = await dbContext.RagIndexingStates.FirstOrDefaultAsync(
            value => value.TenantId == context.TenantId && value.WorkspaceName == context.WorkspaceName && value.SourceName == sourceName,
            cancellationToken);

        return state == null
            ? null
            : new IndexingStatus
            {
                SourceName = state.SourceName,
                TotalDocuments = state.TotalDocuments,
                IndexedDocuments = state.IndexedDocuments,
                LastIndexedAt = state.LastIndexedAt,
                IsIndexing = state.IsIndexing,
                ErrorMessage = state.ErrorMessage
            };
    }

    public async Task UpdateIndexingStatusAsync(VectorStoreContext context, IndexingStatus status, CancellationToken cancellationToken = default)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var state = await dbContext.RagIndexingStates.FirstOrDefaultAsync(
            value => value.TenantId == context.TenantId && value.WorkspaceName == context.WorkspaceName && value.SourceName == status.SourceName,
            cancellationToken);

        if (state == null)
        {
            state = new RagIndexingState(Guid.NewGuid(), context.TenantId, context.WorkspaceName, status.SourceName);
            dbContext.RagIndexingStates.Add(state);
        }

        state.TotalDocuments = status.TotalDocuments;
        state.IndexedDocuments = status.IndexedDocuments;
        state.LastIndexedAt = status.LastIndexedAt;
        state.IsIndexing = status.IsIndexing;
        state.ErrorMessage = status.ErrorMessage;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureCollectionExistsAsync(QdrantClient client, VectorStoreContext context, CancellationToken cancellationToken)
    {
        var exists = await client.CollectionExistsAsync(context.CollectionName, cancellationToken: cancellationToken);
        if (!exists)
        {
            await client.CreateCollectionAsync(
                context.CollectionName,
                new VectorParams { Size = (ulong)context.Dimensions, Distance = Distance.Cosine },
                onDiskPayload: true,
                cancellationToken: cancellationToken);

            await client.CreatePayloadIndexAsync(context.CollectionName, "tenantKey", cancellationToken: cancellationToken);
            await client.CreatePayloadIndexAsync(context.CollectionName, "workspaceName", cancellationToken: cancellationToken);
            await client.CreatePayloadIndexAsync(context.CollectionName, "sourceName", cancellationToken: cancellationToken);
            await client.CreatePayloadIndexAsync(context.CollectionName, "sourceId", cancellationToken: cancellationToken);
            await client.CreatePayloadIndexAsync(context.CollectionName, "documentId", cancellationToken: cancellationToken);
        }
    }

    private static Filter BuildWorkspaceFilter(VectorStoreContext context)
    {
        return MatchKeyword("tenantKey", context.TenantKey) & MatchKeyword("workspaceName", context.WorkspaceName);
    }

    private static Filter BuildDocumentFilter(VectorStoreContext context, string documentId)
    {
        return BuildWorkspaceFilter(context) & MatchKeyword("documentId", documentId);
    }

    private static QdrantClient CreateClient(VectorStoreContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ConnectionString))
        {
            throw new InvalidOperationException("Qdrant connection string is required.");
        }

        if (string.IsNullOrWhiteSpace(context.ApiKey))
        {
            return new QdrantClient(context.ConnectionString);
        }

        var channel = QdrantChannel.ForAddress(context.ConnectionString, new ClientConfiguration
        {
            ApiKey = context.ApiKey
        });
        return new QdrantClient(new QdrantGrpcClient(channel));
    }

    private static DocumentChunk? ToDocumentChunk(VectorStoreContext context, ScoredPoint result)
    {
        var payload = result.Payload;
        if (payload == null || payload.Count == 0)
        {
            return null;
        }

        var documentId = GetString(payload, "documentId");
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return null;
        }

        var metadataJson = GetString(payload, "metadataJson");
        return new DocumentChunk
        {
            Id = documentId,
            WorkspaceName = GetString(payload, "workspaceName") ?? context.WorkspaceName,
            SourceName = GetString(payload, "sourceName") ?? string.Empty,
            SourceId = GetString(payload, "sourceId") ?? string.Empty,
            Content = GetString(payload, "content") ?? string.Empty,
            Metadata = string.IsNullOrWhiteSpace(metadataJson)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson) ?? new Dictionary<string, object>(),
            CreatedAt = ParseDateTime(GetString(payload, "createdAt")) ?? DateTime.MinValue,
            UpdatedAt = ParseDateTime(GetString(payload, "updatedAt")),
            Score = result.Score
        };
    }

    private static string? GetString(MapField<string, Value> payload, string key)
    {
        return payload.TryGetValue(key, out var value) ? value.StringValue : null;
    }

    private static DateTime? ParseDateTime(string? value)
    {
        return DateTime.TryParse(value, out var parsed) ? parsed : null;
    }

    private static void ValidateVectors(VectorStoreContext context, IEnumerable<float[]> vectors)
    {
        foreach (var vector in vectors)
        {
            ValidateVector(context, vector);
        }
    }

    private static void ValidateVector(VectorStoreContext context, float[] vector)
    {
        if (vector.Length != context.Dimensions)
        {
            throw new InvalidOperationException($"Embedding dimension mismatch. Expected {context.Dimensions}, found {vector.Length}.");
        }
    }

    private static ulong GetPointId(string tenantKey, string workspaceName, string sourceName, string sourceId, string documentId)
    {
        var input = $"{tenantKey}::{workspaceName}::{sourceName}::{sourceId}::{documentId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToUInt64(hash, 0);
    }
}
