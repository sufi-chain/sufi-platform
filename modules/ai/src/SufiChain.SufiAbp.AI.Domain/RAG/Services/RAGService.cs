using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI.Adapters;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.Features;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiAbp.AI.RAG.Services;

public class RAGService : DomainService, IRAGService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceSyncService _syncService;
    private readonly IFeatureChecker _featureChecker;
    private readonly IConfiguration _configuration;
    private readonly List<IDocumentSource> _documentSources = new();

    public RAGService(
        IServiceProvider serviceProvider,
        IWorkspaceRepository workspaceRepository,
        WorkspaceSyncService syncService,
        IFeatureChecker featureChecker,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _workspaceRepository = workspaceRepository;
        _syncService = syncService;
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
            return Unavailable("Configure Qdrant or Pgvector to enable RAG.");
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
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = GetVectorStoreContext(workspace);

        var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);
        var embeddings = await embeddingGenerator.GenerateAsync(new[] { query }, cancellationToken: cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        EnsureSourcesLoaded();

        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreContext = GetVectorStoreContext(workspace);
        var source = _documentSources.FirstOrDefault(s => s.SourceName == sourceName);

        if (source == null)
        {
            throw new BusinessException(AIErrorCodes.DocumentSourceNotFound)
                .WithData("SourceName", sourceName);
        }

        var indexingProgress = new IndexingProgress
        {
            SourceName = sourceName,
            StartedAt = Clock.Now
        };

        try
        {
            indexingProgress.TotalDocuments = await source.GetTotalCountAsync(cancellationToken);
            progress?.Report(indexingProgress);

            var documents = await source.SearchAsync(null, indexingProgress.TotalDocuments, cancellationToken);
            var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);
            var texts = documents.Select(d => d.Content).ToList();
            var allEmbeddings = await embeddingGenerator.GenerateAsync(texts, cancellationToken: cancellationToken);

            var index = 0;
            foreach (var embedding in allEmbeddings)
            {
                documents[index].WorkspaceName = workspaceName;
                documents[index].Embedding = embedding.Vector.ToArray();
                indexingProgress.IndexedDocuments = index + 1;
                indexingProgress.CurrentDocument = documents[index].Id;
                progress?.Report(indexingProgress);
                index++;
            }

            var vectorStoreProvider = GetVectorStoreProvider(vectorStoreContext.Type);
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
            indexingProgress.FailedDocuments = indexingProgress.TotalDocuments - indexingProgress.IndexedDocuments;
            await GetVectorStoreProvider(vectorStoreContext.Type).UpdateIndexingStatusAsync(
                vectorStoreContext,
                new IndexingStatus
                {
                    SourceName = sourceName,
                    TotalDocuments = indexingProgress.TotalDocuments,
                    IndexedDocuments = indexingProgress.IndexedDocuments,
                    LastIndexedAt = Clock.Now,
                    IsIndexing = false,
                    ErrorMessage = ex.Message
                },
                cancellationToken);

            throw new BusinessException(AIErrorCodes.EmbeddingGenerationFailed)
                .WithData("Error", ex.Message);
        }
    }

    public async Task IndexAllDocumentsAsync(
        string workspaceName,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        await EnsureRagAvailableAsync(cancellationToken);
        EnsureSourcesLoaded();

        foreach (var source in _documentSources)
        {
            await IndexDocumentsAsync(workspaceName, source.SourceName, progress, cancellationToken);
        }
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
        var vectorStoreContext = GetVectorStoreContext(workspace);
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

    private VectorStoreContext GetVectorStoreContext(Workspace workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace.EmbedderConfigJson))
        {
            throw new BusinessException(AIErrorCodes.EmbedderConfigurationMissing)
                .WithData("WorkspaceName", workspace.Name);
        }

        var vectorStoreConfig = ResolveVectorStoreConfiguration(workspace);
        if (string.IsNullOrWhiteSpace(vectorStoreConfig.CollectionName))
        {
            throw new BusinessException(AIErrorCodes.VectorStoreConfigurationInvalid)
                .WithData("Reason", "CollectionName is required");
        }

        return new VectorStoreContext
        {
            WorkspaceName = workspace.Name,
            Type = vectorStoreConfig.Type,
            CollectionName = vectorStoreConfig.CollectionName,
            ConnectionString = vectorStoreConfig.ConnectionString,
            ApiKey = vectorStoreConfig.ApiKey,
            Dimensions = vectorStoreConfig.Dimensions,
            TenantId = CurrentTenant.Id,
            TenantKey = GetTenantKey(CurrentTenant.Id),
            Schema = vectorStoreConfig.Schema,
            TableName = vectorStoreConfig.TableName,
            ProviderName = vectorStoreConfig.ProviderName
        };
    }

    private VectorStoreConfiguration ResolveVectorStoreConfiguration(Workspace workspace)
    {
        var workspaceConfig = WorkspaceConfigurationHelper.ParseVectorStoreConfig(workspace);
        var platformQdrantConfig = GetQdrantConfiguration();
        var platformPgvectorConfig = GetPgvectorConfiguration();

        if (workspaceConfig != null)
        {
            ApplyDefaults(workspaceConfig, workspace.Name);
            var platformConfig = workspaceConfig.Type switch
            {
                VectorStoreType.Qdrant => platformQdrantConfig,
                VectorStoreType.Pgvector => platformPgvectorConfig,
                _ => null
            };

            if (platformConfig == null && string.IsNullOrWhiteSpace(workspaceConfig.ConnectionString))
            {
                throw new BusinessException(AIErrorCodes.VectorStoreConfigurationMissing)
                    .WithData("WorkspaceName", workspace.Name)
                    .WithData("VectorStoreType", workspaceConfig.Type.ToString());
            }

            return MergeVectorStoreConfigurations(platformConfig, workspaceConfig, workspace.Name);
        }

        var configuredProvider = GetConfiguredPlatformProvider();
        if (configuredProvider == null)
        {
            throw new BusinessException(AIErrorCodes.RagUnavailable)
                .WithData("WorkspaceName", workspace.Name);
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
            CollectionName = _configuration["VectorStore:Qdrant:CollectionName"] ?? "ai_documents",
            Dimensions = ParseDimension(_configuration["VectorStore:Qdrant:Dimensions"])
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
            Dimensions = ParseDimension(_configuration["VectorStore:Pgvector:Dimensions"]),
            Schema = _configuration["VectorStore:Pgvector:Schema"] ?? "rag",
            TableName = _configuration["VectorStore:Pgvector:TableName"] ?? "document_chunks",
            ProviderName = _configuration["VectorStore:Pgvector:ProviderName"] ?? "Npgsql"
        };
    }

    private static VectorStoreConfiguration MergeVectorStoreConfigurations(
        VectorStoreConfiguration? platformConfig,
        VectorStoreConfiguration workspaceConfig,
        string workspaceName)
    {
        if (platformConfig == null)
        {
            ApplyDefaults(workspaceConfig, workspaceName);
            return workspaceConfig;
        }

        return new VectorStoreConfiguration
        {
            Type = workspaceConfig.Type,
            ConnectionString = string.IsNullOrWhiteSpace(workspaceConfig.ConnectionString)
                ? platformConfig.ConnectionString
                : workspaceConfig.ConnectionString,
            ApiKey = string.IsNullOrWhiteSpace(workspaceConfig.ApiKey)
                ? platformConfig.ApiKey
                : workspaceConfig.ApiKey,
            CollectionName = string.IsNullOrWhiteSpace(workspaceConfig.CollectionName)
                ? platformConfig.CollectionName
                : workspaceConfig.CollectionName,
            Dimensions = workspaceConfig.Dimensions > 0 ? workspaceConfig.Dimensions : platformConfig.Dimensions,
            Schema = string.IsNullOrWhiteSpace(workspaceConfig.Schema) ? platformConfig.Schema : workspaceConfig.Schema,
            TableName = string.IsNullOrWhiteSpace(workspaceConfig.TableName) ? platformConfig.TableName : workspaceConfig.TableName,
            ProviderName = string.IsNullOrWhiteSpace(workspaceConfig.ProviderName) ? platformConfig.ProviderName : workspaceConfig.ProviderName
        };
    }

    private static void ApplyDefaults(VectorStoreConfiguration configuration, string workspaceName)
    {
        configuration.CollectionName = string.IsNullOrWhiteSpace(configuration.CollectionName)
            ? GetDefaultCollectionName(workspaceName)
            : configuration.CollectionName;
        configuration.Dimensions = configuration.Dimensions > 0 ? configuration.Dimensions : 1536;

        if (configuration.Type == VectorStoreType.Pgvector)
        {
            configuration.Schema ??= "rag";
            configuration.TableName ??= "document_chunks";
            configuration.ProviderName ??= "Npgsql";
        }
    }

    private static int ParseDimension(string? value)
    {
        return int.TryParse(value, out var dimensions) && dimensions > 0 ? dimensions : 1536;
    }

    private static string GetDefaultCollectionName(string workspaceName)
    {
        var sanitized = new string(workspaceName
            .Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '_')
            .ToArray())
            .Trim('_');

        return string.IsNullOrWhiteSpace(sanitized)
            ? "ai_documents"
            : $"ai_{sanitized}";
    }

    private static string GetTenantKey(Guid? tenantId)
    {
        return tenantId?.ToString("N").ToLowerInvariant() ?? "host";
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
            .WithData("Message", availability.Message ?? "Configure Qdrant or Pgvector to enable RAG.");
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
}
