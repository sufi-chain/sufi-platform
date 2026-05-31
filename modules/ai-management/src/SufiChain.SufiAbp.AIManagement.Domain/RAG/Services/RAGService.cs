using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.RAG.Services;

public class RAGService : DomainService, IRAGService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceSyncService _syncService;
    private readonly IFeatureChecker _featureChecker;
    private readonly List<IDocumentSource> _documentSources = new();

    public RAGService(
        IServiceProvider serviceProvider,
        IWorkspaceRepository workspaceRepository,
        WorkspaceSyncService syncService,
        IFeatureChecker featureChecker)
    {
        _serviceProvider = serviceProvider;
        _workspaceRepository = workspaceRepository;
        _syncService = syncService;
        _featureChecker = featureChecker;
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

    public async Task<List<DocumentChunk>> SearchAsync(
        string workspaceName,
        string query,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        
        if (workspace.EmbedderConfigJson == null || workspace.VectorStoreConfigJson == null)
        {
            throw new BusinessException(AIManagementErrorCodes.InvalidProviderConfiguration)
                .WithData("Reason", "Workspace does not have embedder or vector store configured");
        }

        // Get embedding generator from sync service
        var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);
        
        // Generate query embedding
        var embeddings = await embeddingGenerator.GenerateAsync(new[] { query }, cancellationToken: cancellationToken);
        var queryEmbedding = embeddings.First().Vector.ToArray();

        // Get vector store provider
        var vectorStoreProvider = GetVectorStoreProvider();
        
        // Search similar documents
        var vectorStoreConfig = System.Text.Json.JsonSerializer.Deserialize<VectorStoreConfig>(workspace.VectorStoreConfigJson);
        var results = await vectorStoreProvider.SearchSimilarAsync(
            vectorStoreConfig?.CollectionName ?? "ai_documents",
            queryEmbedding,
            maxResults,
            0.7f,
            cancellationToken
        );

        return results;
    }

    public async Task IndexDocumentsAsync(
        string workspaceName,
        string sourceName,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        EnsureSourcesLoaded();
        
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var source = _documentSources.FirstOrDefault(s => s.SourceName == sourceName);
        
        if (source == null)
        {
            throw new BusinessException(AIManagementErrorCodes.DocumentSourceNotFound)
                .WithData("SourceName", sourceName);
        }

        var indexingProgress = new IndexingProgress
        {
            SourceName = sourceName,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            // Get total count
            indexingProgress.TotalDocuments = await source.GetTotalCountAsync(cancellationToken);
            progress?.Report(indexingProgress);

            // Fetch documents
            var documents = await source.SearchAsync(null, indexingProgress.TotalDocuments, cancellationToken);

            // Get embedding generator from sync service
            var embeddingGenerator = await _syncService.GetOrCreateEmbeddingGeneratorAsync(workspaceName, cancellationToken);

            // Generate embeddings in batches
            var texts = documents.Select(d => d.Content).ToList();
            var allEmbeddings = await embeddingGenerator.GenerateAsync(texts, cancellationToken: cancellationToken);

            // Attach embeddings to documents
            int i = 0;
            foreach (var embedding in allEmbeddings)
            {
                documents[i].Embedding = embedding.Vector.ToArray();
                indexingProgress.IndexedDocuments = i + 1;
                indexingProgress.CurrentDocument = documents[i].Id;
                progress?.Report(indexingProgress);
                i++;
            }

            // Store in vector store
            var vectorStoreProvider = GetVectorStoreProvider();
            var vectorStoreConfig = System.Text.Json.JsonSerializer.Deserialize<VectorStoreConfig>(workspace.VectorStoreConfigJson!);
            
            await vectorStoreProvider.StoreEmbeddingsAsync(
                vectorStoreConfig?.CollectionName ?? "ai_documents",
                documents,
                cancellationToken
            );

            indexingProgress.CompletedAt = DateTime.UtcNow;
            progress?.Report(indexingProgress);
        }
        catch (Exception ex)
        {
            indexingProgress.FailedDocuments = indexingProgress.TotalDocuments - indexingProgress.IndexedDocuments;
            throw new BusinessException(AIManagementErrorCodes.EmbeddingGenerationFailed)
                .WithData("Error", ex.Message);
        }
    }

    public async Task IndexAllDocumentsAsync(
        string workspaceName,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
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
        EnsureSourcesLoaded();
        
        var source = _documentSources.FirstOrDefault(s => s.SourceName == sourceName);
        
        if (source == null)
        {
            throw new BusinessException(AIManagementErrorCodes.DocumentSourceNotFound)
                .WithData("SourceName", sourceName);
        }

        var totalDocuments = await source.GetTotalCountAsync(cancellationToken);
        
        // Get vector store count
        var workspace = await GetWorkspaceByNameAsync(workspaceName);
        var vectorStoreProvider = GetVectorStoreProvider();
        var vectorStoreConfig = System.Text.Json.JsonSerializer.Deserialize<VectorStoreConfig>(workspace.VectorStoreConfigJson ?? "{}");
        
        var indexedCount = await vectorStoreProvider.GetCountAsync(
            vectorStoreConfig?.CollectionName ?? "ai_documents",
            cancellationToken
        );

        return new IndexingStatus
        {
            SourceName = sourceName,
            TotalDocuments = totalDocuments,
            IndexedDocuments = indexedCount,
            LastIndexedAt = DateTime.UtcNow,
            IsIndexing = false
        };
    }

    private void EnsureSourcesLoaded()
    {
        if (_documentSources.Count == 0)
        {
            var sources = _serviceProvider.GetServices<IDocumentSource>();
            foreach (var source in sources)
            {
                RegisterDocumentSource(source);
            }
        }
    }

    private async Task CheckFeatureAsync()
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpAIFeatures.Enable}");
        }

        if (!await _featureChecker.IsEnabledAsync(SufiAbpAIFeatures.RAG))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpAIFeatures.RAG}");
        }
    }

    private async Task<Workspace> GetWorkspaceByNameAsync(string workspaceName)
    {
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName);
        
        if (workspace == null)
        {
            throw new BusinessException(AIManagementErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        return workspace;
    }

    private IVectorStoreProvider GetVectorStoreProvider()
    {
        var providers = _serviceProvider.GetServices<IVectorStoreProvider>();
        var provider = providers.FirstOrDefault(p => p.Type == VectorStoreType.MongoDB);
        
        if (provider == null)
        {
            throw new BusinessException(AIManagementErrorCodes.InvalidProviderConfiguration)
                .WithData("VectorStore", "MongoDB");
        }

        return provider;
    }

    private class VectorStoreConfig
    {
        public string CollectionName { get; set; } = "ai_documents";
    }
}
