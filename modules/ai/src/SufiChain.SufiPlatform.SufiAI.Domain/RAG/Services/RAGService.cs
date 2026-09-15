using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using SufiChain.SufiPlatform.SufiAI.Adapters;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.SufiAI.RAG.Services;

public class RAGService : DomainService, IRAGService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceSyncService _syncService;
    private readonly IWorkspaceEmbedderResolver _embedderResolver;
    private readonly IFeatureChecker _featureChecker;
    private readonly IConfiguration _configuration;
    private readonly List<IDocumentSource> _documentSources = new();

    public RAGService(
        IServiceProvider serviceProvider,
        IWorkspaceRepository workspaceRepository,
        WorkspaceSyncService syncService,
        IWorkspaceEmbedderResolver embedderResolver,
        IFeatureChecker featureChecker,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _workspaceRepository = workspaceRepository;
        _syncService = syncService;
        _embedderResolver = embedderResolver;
        _featureChecker = featureChecker;
        _configuration = configuration;
    }

    public void RegisterDocumentSource(IDocumentSource source)
    {
        if (!_documentSources.Any(s => s.SourceName == source.SourceName))
        {
            _documentSources.Add(source);
        }
    }

    public List<IDocumentSource> GetDocumentSources()
    {
        // Sync bridge: IDocumentSource registry is consumed synchronously by Semantic Kernel plugins;
        // feature check is async-only — blocking here is intentional (DEBT-009).
        CheckFeatureAsync().GetAwaiter().GetResult();
        EnsureSourcesLoaded();
        return _documentSources.ToList();
    }

    public async Task<RagAvailability> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();

        var configuredProvider = GetConfiguredPlatformProvider();
        if (configuredProvider == null)
        {
            return Unavailable("Configure exactly one of Qdrant or Pgvector under VectorStore host settings to enable RAG.");
        }

        if (GetVectorStoreProviderOrNull(configuredProvider.Type) == null)
        {
            return Unavailable($"The configured {configuredProvider.Type} provider is not registered.");
        }

        return new RagAvailability
        {
            IsAvailable = true,
            Provider = configuredProvider.Type == VectorStoreType.Qdrant
                ? RagProviderKind.Qdrant
                : RagProviderKind.Pgvector
        };
    }

    public async Task<List<DocumentChunk>> SearchAsync(
        string workspaceName,
        string query,
        int maxResults = 10,
        float? minSimilarity = null,
        string? sourceName = null,
        IReadOnlyDictionary<string, string>? metadataFilters = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = await GetVectorStoreContextAsync(workspace, cancellationToken);
        vectorStoreContext.SourceName = string.IsNullOrWhiteSpace(sourceName) ? null : sourceName.Trim();
        vectorStoreContext.MetadataFilters = DocumentChunkMetadataFilter.Normalize(metadataFilters);

        var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);
        var queryText = EmbeddingInputGuard.FitToBudget(query, vectorStoreContext.MaxInputTokens);
        var embeddings = await embeddingGenerator.GenerateAsync(new[] { queryText }, cancellationToken: cancellationToken);
        var queryEmbedding = embeddings.First().Vector.ToArray();

        var vectorStoreProvider = GetVectorStoreProvider(vectorStoreContext.Type);

        return await vectorStoreProvider.SearchSimilarAsync(
            vectorStoreContext,
            queryEmbedding,
            maxResults,
            minSimilarity ?? 0.7f,
            cancellationToken
        );
    }

    public async Task IndexDocumentsAsync(
        string workspaceName,
        string sourceName,
        IProgress<IndexingProgress>? progress = null,
        IReadOnlyDictionary<string, string>? metadataFilters = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        EnsureSourcesLoaded();

        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = await GetVectorStoreContextAsync(workspace, cancellationToken);
        var source = _documentSources.FirstOrDefault(s => s.SourceName == sourceName);

        if (source == null)
        {
            throw new BusinessException(AIErrorCodes.DocumentSourceNotFound)
                .WithData("SourceName", sourceName);
        }

        var normalizedFilters = DocumentChunkMetadataFilter.Normalize(metadataFilters);

        var indexingProgress = new IndexingProgress
        {
            SourceName = sourceName,
            StartedAt = Clock.Now
        };

        var phase = "LoadDocuments";
        try
        {
            var harvestCount = await source.GetTotalCountAsync(cancellationToken);
            progress?.Report(indexingProgress);

            var documents = await source.SearchAsync(null, Math.Max(harvestCount, 1), cancellationToken);
            documents = DocumentChunkMetadataFilter.Filter(documents, normalizedFilters);
            documents = EmbeddingInputGuard.SplitDocuments(documents, vectorStoreContext.MaxInputTokens);
            indexingProgress.TotalDocuments = documents.Count;
            progress?.Report(indexingProgress);

            var vectorStoreProvider = GetVectorStoreProvider(vectorStoreContext.Type);

            if (documents.Count == 0)
            {
                indexingProgress.CompletedAt = Clock.Now;
                progress?.Report(indexingProgress);
                await vectorStoreProvider.UpdateIndexingStatusAsync(
                    vectorStoreContext,
                    new IndexingStatus
                    {
                        SourceName = sourceName,
                        TotalDocuments = 0,
                        IndexedDocuments = 0,
                        LastIndexedAt = Clock.Now,
                        IsIndexing = false
                    },
                    cancellationToken);
                return;
            }

            phase = "GenerateEmbeddings";
            var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);
            await AssignEmbeddingsAsync(
                documents,
                embeddingGenerator,
                workspaceName,
                indexingProgress,
                progress,
                cancellationToken);

            phase = "StoreEmbeddings";
            await vectorStoreProvider.StoreEmbeddingsAsync(vectorStoreContext, documents, cancellationToken);

            indexingProgress.CompletedAt = Clock.Now;
            progress?.Report(indexingProgress);
            await vectorStoreProvider.UpdateIndexingStatusAsync(
                vectorStoreContext,
                new IndexingStatus
                {
                    SourceName = sourceName,
                    TotalDocuments = indexingProgress.TotalDocuments,
                    IndexedDocuments = indexingProgress.IndexedDocuments,
                    LastIndexedAt = Clock.Now,
                    IsIndexing = false
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            var detail = ResolveExceptionDetail(ex);
            Logger.LogError(
                ex,
                "RAG indexing failed. WorkspaceName={WorkspaceName}, SourceName={SourceName}, Phase={Phase}, DocumentCount={DocumentCount}, IndexedDocuments={IndexedDocuments}, Error={Error}",
                workspaceName,
                sourceName,
                phase,
                indexingProgress.TotalDocuments,
                indexingProgress.IndexedDocuments,
                detail);

            indexingProgress.FailedDocuments = indexingProgress.TotalDocuments - indexingProgress.IndexedDocuments;
            try
            {
                await GetVectorStoreProvider(vectorStoreContext.Type).UpdateIndexingStatusAsync(
                    vectorStoreContext,
                    new IndexingStatus
                    {
                        SourceName = sourceName,
                        TotalDocuments = indexingProgress.TotalDocuments,
                        IndexedDocuments = indexingProgress.IndexedDocuments,
                        LastIndexedAt = Clock.Now,
                        IsIndexing = false,
                        ErrorMessage = detail
                    },
                    cancellationToken);
            }
            catch (Exception statusEx)
            {
                Logger.LogWarning(
                    statusEx,
                    "Failed to persist RAG indexing failure status. WorkspaceName={WorkspaceName}, SourceName={SourceName}",
                    workspaceName,
                    sourceName);
            }

            if (ex is BusinessException)
            {
                throw;
            }

            throw CreateIndexingBusinessException(phase, workspaceName, sourceName, ex);
        }
    }

    public async Task IndexAllDocumentsAsync(
        string workspaceName,
        IProgress<IndexingProgress>? progress = null,
        IReadOnlyDictionary<string, string>? metadataFilters = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        EnsureSourcesLoaded();

        foreach (var source in _documentSources)
        {
            await IndexDocumentsAsync(
                workspaceName,
                source.SourceName,
                progress,
                metadataFilters,
                cancellationToken);
        }
    }

    public async Task<DocumentIndexResult> IndexDocumentAsync(
        string workspaceName,
        string sourceName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        EnsureSourcesLoaded();

        var normalizedDocumentId = Check.NotNullOrWhiteSpace(documentId, nameof(documentId)).Trim();
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = await GetVectorStoreContextAsync(workspace, cancellationToken);
        var source = _documentSources.FirstOrDefault(s => s.SourceName == sourceName);
        if (source == null)
        {
            throw new BusinessException(AIErrorCodes.DocumentSourceNotFound)
                .WithData("SourceName", sourceName);
        }

        var vectorStoreProvider = GetVectorStoreProvider(vectorStoreContext.Type);
        var phase = "LoadDocuments";
        try
        {
            var document = await source.GetByIdAsync(normalizedDocumentId, cancellationToken);
            if (document == null)
            {
                // The source no longer exposes the document (unpublished, deleted, or filtered out):
                // stale vectors must not stay retrievable.
                phase = "StoreEmbeddings";
                await vectorStoreProvider.DeleteBySourceAsync(vectorStoreContext, sourceName, normalizedDocumentId, cancellationToken);
                return new DocumentIndexResult
                {
                    DocumentId = normalizedDocumentId,
                    StoredChunkCount = 0,
                    Removed = true
                };
            }

            var documents = EmbeddingInputGuard.SplitDocuments(new[] { document }, vectorStoreContext.MaxInputTokens);

            phase = "GenerateEmbeddings";
            var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);
            await AssignEmbeddingsAsync(
                documents,
                embeddingGenerator,
                workspaceName,
                new IndexingProgress { SourceName = sourceName, StartedAt = Clock.Now, TotalDocuments = documents.Count },
                progress: null,
                cancellationToken);

            phase = "StoreEmbeddings";
            // Replace instead of append so a shorter re-chunked document leaves no orphan chunks behind.
            await vectorStoreProvider.DeleteBySourceAsync(
                vectorStoreContext,
                sourceName,
                string.IsNullOrWhiteSpace(document.SourceId) ? normalizedDocumentId : document.SourceId,
                cancellationToken);
            await vectorStoreProvider.StoreEmbeddingsAsync(vectorStoreContext, documents, cancellationToken);

            return new DocumentIndexResult
            {
                DocumentId = normalizedDocumentId,
                StoredChunkCount = documents.Count,
                Removed = false
            };
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            Logger.LogError(
                ex,
                "RAG document indexing failed. WorkspaceName={WorkspaceName}, SourceName={SourceName}, DocumentId={DocumentId}, Phase={Phase}",
                workspaceName,
                sourceName,
                normalizedDocumentId,
                phase);
            throw CreateIndexingBusinessException(phase, workspaceName, sourceName, ex);
        }
    }

    public async Task<int> CountAsync(
        string workspaceName,
        string? sourceName = null,
        IReadOnlyDictionary<string, string>? metadataFilters = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);

        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = await GetVectorStoreContextAsync(workspace, cancellationToken);
        vectorStoreContext.SourceName = string.IsNullOrWhiteSpace(sourceName) ? null : sourceName.Trim();
        vectorStoreContext.MetadataFilters = DocumentChunkMetadataFilter.Normalize(metadataFilters);

        var vectorStoreProvider = GetVectorStoreProvider(vectorStoreContext.Type);
        return await vectorStoreProvider.GetCountAsync(vectorStoreContext, cancellationToken);
    }

    public async Task<IndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        EnsureSourcesLoaded();

        var source = _documentSources.FirstOrDefault(s => s.SourceName == sourceName);
        if (source == null)
        {
            throw new BusinessException(AIErrorCodes.DocumentSourceNotFound)
                .WithData("SourceName", sourceName);
        }

        var totalDocuments = await source.GetTotalCountAsync(cancellationToken);
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = await GetVectorStoreContextAsync(workspace, cancellationToken);
        var vectorStoreProvider = GetVectorStoreProvider(vectorStoreContext.Type);

        var indexedCount = await vectorStoreProvider.GetCountAsync(vectorStoreContext, cancellationToken);
        var persistedStatus = await vectorStoreProvider.GetIndexingStatusAsync(vectorStoreContext, sourceName, cancellationToken);

        return new IndexingStatus
        {
            SourceName = sourceName,
            TotalDocuments = totalDocuments,
            IndexedDocuments = indexedCount,
            LastIndexedAt = persistedStatus?.LastIndexedAt,
            IsIndexing = persistedStatus?.IsIndexing ?? false,
            ErrorMessage = persistedStatus?.ErrorMessage
        };
    }

    private static async Task AssignEmbeddingsAsync(
        List<DocumentChunk> documents,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        string workspaceName,
        IndexingProgress indexingProgress,
        IProgress<IndexingProgress>? progress,
        CancellationToken cancellationToken)
    {
        const int batchSize = 32;
        var offset = 0;
        while (offset < documents.Count)
        {
            var batchCount = Math.Min(batchSize, documents.Count - offset);
            var batch = documents.GetRange(offset, batchCount);
            var embeddings = await embeddingGenerator.GenerateAsync(
                batch.Select(document => document.Content),
                cancellationToken: cancellationToken);

            var batchIndex = 0;
            foreach (var embedding in embeddings)
            {
                var document = documents[offset + batchIndex];
                document.WorkspaceName = workspaceName;
                document.Embedding = embedding.Vector.ToArray();
                indexingProgress.IndexedDocuments = offset + batchIndex + 1;
                indexingProgress.CurrentDocument = document.Id;
                progress?.Report(indexingProgress);
                batchIndex++;
            }

            if (batchIndex != batchCount)
            {
                throw new InvalidOperationException(
                    $"Expected {batchCount} text embedding(s), but received {batchIndex}.");
            }

            offset += batchCount;
        }
    }

    private void EnsureSourcesLoaded()
    {
        if (_documentSources.Count != 0)
        {
            return;
        }

        foreach (var source in _serviceProvider.GetServices<IDocumentSource>())
        {
            RegisterDocumentSource(source);
        }

        foreach (var source in _serviceProvider.GetServices<ISufiAIDocumentSource>())
        {
            RegisterDocumentSource(new SufiAIDocumentSourceAdapter(source));
        }
    }

    private async Task CheckFeatureAsync()
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAIFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiAIFeatures.Enable}");
        }

        if (!await _featureChecker.IsEnabledAsync(SufiAIFeatures.RAG))
        {
            throw new BusinessException($"Feature is disabled: {SufiAIFeatures.RAG}");
        }
    }

    private async Task<Workspace> GetWorkspaceByNameAsync(string workspaceName)
    {
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        return workspace;
    }

    private IVectorStoreProvider GetVectorStoreProvider(VectorStoreType vectorStoreType)
    {
        var provider = GetVectorStoreProviderOrNull(vectorStoreType);
        if (provider == null)
        {
            throw new BusinessException(AIErrorCodes.VectorStoreProviderNotSupported)
                .WithData("VectorStore", vectorStoreType.ToString());
        }

        return provider;
    }

    private IVectorStoreProvider? GetVectorStoreProviderOrNull(VectorStoreType vectorStoreType)
    {
        return _serviceProvider
            .GetServices<IVectorStoreProvider>()
            .FirstOrDefault(provider => provider.Type == vectorStoreType);
    }

    private async Task<VectorStoreContext> GetVectorStoreContextAsync(
        Workspace workspace,
        CancellationToken cancellationToken)
    {
        var embedderConfiguration = await _embedderResolver.ResolveAsync(workspace, cancellationToken);

        var vectorStoreConfig = ResolvePlatformVectorStoreConfiguration(workspace.Name);
        if (string.IsNullOrWhiteSpace(vectorStoreConfig.CollectionName))
        {
            throw new BusinessException(AIErrorCodes.VectorStoreConfigurationInvalid)
                .WithData("Reason", "CollectionName is required");
        }

        var tenantKey = VectorStoreTenantScope.GetTenantKey(CurrentTenant.Id);
        var fingerprint = BuildEmbedderFingerprint(embedderConfiguration, vectorStoreConfig);
        var collectionName = VectorStoreTenantScope.BuildName(
            $"{vectorStoreConfig.CollectionName}_{fingerprint}",
            tenantKey);
        var schema = string.IsNullOrWhiteSpace(vectorStoreConfig.Schema)
            ? null
            : VectorStoreTenantScope.BuildName(vectorStoreConfig.Schema, tenantKey);

        return new VectorStoreContext
        {
            WorkspaceName = workspace.Name,
            Type = vectorStoreConfig.Type,
            CollectionName = collectionName,
            ConnectionString = vectorStoreConfig.ConnectionString,
            ApiKey = vectorStoreConfig.ApiKey,
            Dimensions = embedderConfiguration.Dimensions,
            MaxInputTokens = embedderConfiguration.MaxInputTokens,
            TenantId = CurrentTenant.Id,
            TenantKey = tenantKey,
            Schema = schema,
            TableName = string.IsNullOrWhiteSpace(vectorStoreConfig.TableName)
                ? null
                : VectorStoreTenantScope.BuildName(
                    $"{vectorStoreConfig.TableName}_{fingerprint}",
                    tenantKey),
            EmbedderFingerprint = fingerprint,
            ProviderName = vectorStoreConfig.ProviderName
        };
    }

    private static string BuildEmbedderFingerprint(
        EmbedderConfiguration embedderConfiguration,
        VectorStoreConfiguration vectorStoreConfiguration)
    {
        var material = string.Join(
            "|",
            embedderConfiguration.Provider,
            embedderConfiguration.Model.Trim(),
            embedderConfiguration.Dimensions,
            embedderConfiguration.EncodingFormat ?? string.Empty,
            embedderConfiguration.ApiBaseUrl?.TrimEnd('/') ?? string.Empty,
            vectorStoreConfiguration.Type);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }

    private VectorStoreConfiguration ResolvePlatformVectorStoreConfiguration(string workspaceName)
    {
        var configuredProvider = GetConfiguredPlatformProvider();
        if (configuredProvider == null)
        {
            throw new BusinessException(AIErrorCodes.RagUnavailable)
                .WithData("WorkspaceName", workspaceName)
                .WithData("Message", "Configure exactly one of Qdrant or Pgvector under VectorStore host settings.");
        }

        return configuredProvider;
    }

    private VectorStoreConfiguration? GetConfiguredPlatformProvider()
    {
        var qdrantConfig = GetQdrantConfiguration();
        var pgvectorConfig = GetPgvectorConfiguration();

        if (qdrantConfig != null && pgvectorConfig != null)
        {
            return null;
        }

        return qdrantConfig ?? pgvectorConfig;
    }

    private VectorStoreConfiguration? GetQdrantConfiguration()
    {
        var connectionString = _configuration["VectorStore:Qdrant:ConnectionString"] ?? _configuration["VectorStore:Qdrant:Url"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        return new VectorStoreConfiguration
        {
            Type = VectorStoreType.Qdrant,
            ConnectionString = connectionString,
            ApiKey = _configuration["VectorStore:Qdrant:ApiKey"],
            CollectionName = _configuration["VectorStore:Qdrant:CollectionName"] ?? "ai_documents"
        };
    }

    private VectorStoreConfiguration? GetPgvectorConfiguration()
    {
        var connectionString = _configuration["VectorStore:Pgvector:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        return new VectorStoreConfiguration
        {
            Type = VectorStoreType.Pgvector,
            ConnectionString = connectionString,
            CollectionName = _configuration["VectorStore:Pgvector:CollectionName"] ?? "ai_documents",
            Schema = _configuration["VectorStore:Pgvector:Schema"] ?? "rag",
            TableName = _configuration["VectorStore:Pgvector:TableName"] ?? "document_chunks",
            ProviderName = _configuration["VectorStore:Pgvector:ProviderName"] ?? "Npgsql"
        };
    }

    private async Task EnsureRagAvailableAsync(CancellationToken cancellationToken)
    {
        var availability = await GetAvailabilityAsync(cancellationToken);
        if (availability.IsAvailable)
        {
            return;
        }

        throw new BusinessException(AIErrorCodes.RagUnavailable)
            .WithData("Provider", availability.Provider.ToString())
            .WithData("Message", availability.Message ?? "Configure exactly one of Qdrant or Pgvector under VectorStore host settings.");
    }

    private static RagAvailability Unavailable(string message)
    {
        return new RagAvailability
        {
            IsAvailable = false,
            Provider = RagProviderKind.None,
            Message = message
        };
    }

    private static BusinessException CreateIndexingBusinessException(
        string phase,
        string workspaceName,
        string sourceName,
        Exception exception)
    {
        var detail = ResolveExceptionDetail(exception);
        var code = phase switch
        {
            "GenerateEmbeddings" => AIErrorCodes.EmbeddingGenerationFailed,
            "StoreEmbeddings" => AIErrorCodes.VectorStoreWriteFailed,
            _ => AIErrorCodes.DocumentIndexingFailed
        };

        var businessException = new BusinessException(code, detail, innerException: exception);
        businessException
            .WithData("WorkspaceName", workspaceName)
            .WithData("SourceName", sourceName)
            .WithData("Phase", phase)
            .WithData("Error", detail);
        return businessException;
    }

    private static string ResolveExceptionDetail(Exception exception)
    {
        if (exception is AggregateException aggregate)
        {
            var innerMessages = aggregate.Flatten().InnerExceptions
                .Select(inner => inner.Message)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();

            if (innerMessages.Count > 0)
            {
                return string.Join(" | ", innerMessages);
            }
        }

        var detail = exception.GetBaseException().Message;
        if (string.IsNullOrWhiteSpace(detail) ||
            (detail.StartsWith("Exception of type '", StringComparison.Ordinal) &&
             detail.EndsWith("' was thrown.", StringComparison.Ordinal)))
        {
            detail = exception.Message;
        }

        return string.IsNullOrWhiteSpace(detail) ? exception.GetType().Name : detail;
    }
}
