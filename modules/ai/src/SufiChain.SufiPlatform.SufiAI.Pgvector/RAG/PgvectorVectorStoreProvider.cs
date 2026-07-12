using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.Pgvector;

public class PgvectorVectorStoreProvider : IVectorStoreProvider, ITransientDependency
{
    private static readonly ConcurrentDictionary<string, NpgsqlDataSource> DataSources = new(StringComparer.Ordinal);
    private readonly IDbContextProvider<AIDbContext> _dbContextProvider;

    public VectorStoreType Type => VectorStoreType.Pgvector;

    public PgvectorVectorStoreProvider(IDbContextProvider<AIDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task StoreEmbeddingsAsync(VectorStoreContext context, List<DocumentChunk> documents, CancellationToken cancellationToken = default)
    {
        ValidateVectors(context, documents.Select(document => document.Embedding).Where(embedding => embedding != null).Cast<float[]>());
        await EnsureSchemaAsync(context, cancellationToken);

        var dataSource = GetDataSource(context);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        foreach (var document in documents)
        {
            await using var command = new NpgsqlCommand($"""
                insert into {GetQualifiedTableName(context)}(
                    tenant_id,
                    tenant_key,
                    workspace_name,
                    source_name,
                    source_id,
                    document_id,
                    content,
                    metadata_json,
                    embedding,
                    created_at,
                    updated_at)
                values (
                    @tenant_id,
                    @tenant_key,
                    @workspace_name,
                    @source_name,
                    @source_id,
                    @document_id,
                    @content,
                    cast(@metadata_json as jsonb),
                    @embedding,
                    @created_at,
                    @updated_at)
                on conflict (tenant_key, workspace_name, source_name, source_id, document_id)
                do update set
                    content = excluded.content,
                    metadata_json = excluded.metadata_json,
                    embedding = excluded.embedding,
                    updated_at = excluded.updated_at;
                """, connection);

            command.Parameters.AddWithValue("tenant_id", (object?)context.TenantId ?? DBNull.Value);
            command.Parameters.AddWithValue("tenant_key", context.TenantKey);
            command.Parameters.AddWithValue("workspace_name", context.WorkspaceName);
            command.Parameters.AddWithValue("source_name", document.SourceName);
            command.Parameters.AddWithValue("source_id", document.SourceId);
            command.Parameters.AddWithValue("document_id", document.Id);
            command.Parameters.AddWithValue("content", document.Content);
            command.Parameters.AddWithValue("metadata_json", JsonSerializer.Serialize(document.Metadata));
            command.Parameters.AddWithValue("embedding", new Vector(document.Embedding ?? Array.Empty<float>()));
            command.Parameters.AddWithValue("created_at", document.CreatedAt);
            command.Parameters.AddWithValue("updated_at", document.UpdatedAt ?? document.CreatedAt);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<List<DocumentChunk>> SearchSimilarAsync(VectorStoreContext context, float[] queryEmbedding, int maxResults = 10, float minSimilarity = 0.7f, CancellationToken cancellationToken = default)
    {
        ValidateVector(context, queryEmbedding);
        await EnsureSchemaAsync(context, cancellationToken);

        var dataSource = GetDataSource(context);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand($"""
            select source_name,
                   source_id,
                   document_id,
                   content,
                   metadata_json::text,
                   created_at,
                   updated_at,
                   1 - (embedding <=> @embedding) as score
            from {GetQualifiedTableName(context)}
            where tenant_key = @tenant_key
              and workspace_name = @workspace_name
              and 1 - (embedding <=> @embedding) >= @min_similarity
            order by embedding <=> @embedding
            limit @max_results;
            """, connection);

        command.Parameters.AddWithValue("embedding", new Vector(queryEmbedding));
        command.Parameters.AddWithValue("tenant_key", context.TenantKey);
        command.Parameters.AddWithValue("workspace_name", context.WorkspaceName);
        command.Parameters.AddWithValue("min_similarity", minSimilarity);
        command.Parameters.AddWithValue("max_results", maxResults);

        var results = new List<DocumentChunk>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DocumentChunk
            {
                WorkspaceName = context.WorkspaceName,
                SourceName = reader.GetString(0),
                SourceId = reader.GetString(1),
                Id = reader.GetString(2),
                Content = reader.GetString(3),
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(4)) ?? new Dictionary<string, object>(),
                CreatedAt = reader.GetDateTime(5),
                UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                Score = Convert.ToSingle(reader.GetValue(7), CultureInfo.InvariantCulture)
            });
        }

        return results;
    }

    public async Task DeleteAsync(VectorStoreContext context, string documentId, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(context, cancellationToken);

        var dataSource = GetDataSource(context);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand($"""
            delete from {GetQualifiedTableName(context)}
            where tenant_key = @tenant_key
              and workspace_name = @workspace_name
              and document_id = @document_id;
            """, connection);

        command.Parameters.AddWithValue("tenant_key", context.TenantKey);
        command.Parameters.AddWithValue("workspace_name", context.WorkspaceName);
        command.Parameters.AddWithValue("document_id", documentId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(VectorStoreContext context, CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(context, cancellationToken);

        var dataSource = GetDataSource(context);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand($"""
            select count(*)
            from {GetQualifiedTableName(context)}
            where tenant_key = @tenant_key
              and workspace_name = @workspace_name;
            """, connection);

        command.Parameters.AddWithValue("tenant_key", context.TenantKey);
        command.Parameters.AddWithValue("workspace_name", context.WorkspaceName);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
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

    private static string GetSchema(VectorStoreContext context) => string.IsNullOrWhiteSpace(context.Schema) ? "rag" : context.Schema!;
    private static string GetTableName(VectorStoreContext context) => string.IsNullOrWhiteSpace(context.TableName) ? "document_chunks" : context.TableName!;
    private static string GetQualifiedTableName(VectorStoreContext context) => $"{QuoteIdentifier(GetSchema(context))}.{QuoteIdentifier(GetTableName(context))}";

    private async Task EnsureSchemaAsync(VectorStoreContext context, CancellationToken cancellationToken)
    {
        var dataSource = GetDataSource(context);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        await using (var extensionCommand = new NpgsqlCommand("create extension if not exists vector;", connection))
        {
            await extensionCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await connection.ReloadTypesAsync();

        await using (var schemaCommand = new NpgsqlCommand($"create schema if not exists {QuoteIdentifier(GetSchema(context))};", connection))
        {
            await schemaCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var tableCommand = new NpgsqlCommand($$$"""
            create table if not exists {GetQualifiedTableName(context)} (
                id bigserial primary key,
                tenant_id uuid null,
                tenant_key text not null,
                workspace_name text not null,
                source_name text not null,
                source_id text not null,
                document_id text not null,
                content text not null,
                metadata_json jsonb not null default '{{}}'::jsonb,
                embedding vector({context.Dimensions}) not null,
                created_at timestamptz not null,
                updated_at timestamptz null
            );
            """, connection))
        {
            await tableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await ValidateTableShapeAsync(context, connection, cancellationToken);

        await using (var lookupIndexCommand = new NpgsqlCommand($"""
            create unique index if not exists {QuoteIdentifier($"{GetTableName(context)}_uq_tenant_ws_doc")}
            on {GetQualifiedTableName(context)}(tenant_key, workspace_name, source_name, source_id, document_id);

            create index if not exists {QuoteIdentifier($"{GetTableName(context)}_ix_tenant_ws")}
            on {GetQualifiedTableName(context)}(tenant_key, workspace_name);
            """, connection))
        {
            await lookupIndexCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var vectorIndexCommand = new NpgsqlCommand($"""
            create index if not exists {QuoteIdentifier($"{GetTableName(context)}_ix_embedding_hnsw")}
            on {GetQualifiedTableName(context)}
            using hnsw (embedding vector_cosine_ops);
            """, connection);
        await vectorIndexCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ValidateTableShapeAsync(VectorStoreContext context, NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var dimensionCommand = new NpgsqlCommand("""
            select format_type(a.atttypid, a.atttypmod)
            from pg_attribute a
            inner join pg_class c on c.oid = a.attrelid
            inner join pg_namespace n on n.oid = c.relnamespace
            where n.nspname = @schema_name
              and c.relname = @table_name
              and a.attname = 'embedding'
              and a.attnum > 0
              and not a.attisdropped;
            """, connection);

        dimensionCommand.Parameters.AddWithValue("schema_name", GetSchema(context));
        dimensionCommand.Parameters.AddWithValue("table_name", GetTableName(context));

        var value = (string?)await dimensionCommand.ExecuteScalarAsync(cancellationToken);
        var expectedType = $"vector({context.Dimensions})";
        if (!string.Equals(value, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Pgvector table dimension mismatch. Expected {expectedType}, found {value ?? "<missing>"}.");
        }
    }

    private static NpgsqlDataSource GetDataSource(VectorStoreContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ConnectionString))
        {
            throw new InvalidOperationException("Pgvector connection string is required.");
        }

        return DataSources.GetOrAdd(context.ConnectionString, connectionString =>
        {
            var builder = new NpgsqlDataSourceBuilder(connectionString);
            builder.UseVector();
            return builder.Build();
        });
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

    private static string QuoteIdentifier(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";
}
