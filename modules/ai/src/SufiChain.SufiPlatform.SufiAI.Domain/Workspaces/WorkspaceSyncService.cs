using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.Features;
using System.Collections.Concurrent;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Synchronizes workspace configuration from the database.
/// Kernels are created per request. Embedding generators are process-local
/// instances keyed by tenant, workspace id, and configuration id, invalidated
/// by a distributed stamp.
/// </summary>
public class WorkspaceSyncService : ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceEmbedderResolver _embedderResolver;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly IDistributedCache<WorkspaceEmbedderCacheStamp> _embedderStampCache;
    private readonly IDistributedCache<WorkspaceProviderModelCacheStamp> _providerModelStampCache;
    private readonly ICurrentTenant _currentTenant;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<WorkspaceSyncService> _logger;

    private static readonly ConcurrentDictionary<string, CachedEmbeddingGenerator> EmbeddingGenerators = new();
    private static readonly SemaphoreSlim EmbeddingConstructionLock = new(1, 1);

    public WorkspaceSyncService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceEmbedderResolver embedderResolver,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        IDistributedCache<WorkspaceEmbedderCacheStamp> embedderStampCache,
        IDistributedCache<WorkspaceProviderModelCacheStamp> providerModelStampCache,
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider,
        IFeatureChecker featureChecker,
        ILogger<WorkspaceSyncService> logger)
    {
        _workspaceRepository = workspaceRepository;
        _embedderResolver = embedderResolver;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _embedderStampCache = embedderStampCache;
        _providerModelStampCache = providerModelStampCache;
        _currentTenant = currentTenant;
        _serviceProvider = serviceProvider;
        _featureChecker = featureChecker;
        _logger = logger;
    }

    public async Task<Kernel> CreateRequestKernelAsync(
        WorkspaceRuntimeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Workspaces);
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_serviceProvider);
        WorkspaceConfigurationHelper.ConfigureKernel(builder, configuration,
            _serviceProvider.GetRequiredService<IOptions<AITransportOptions>>().Value);
        return builder.Build();
    }

    /// <summary>
    /// Gets or creates an embedding generator for the workspace.
    /// Instances stay in-process; a distributed stamp invalidates them across hosts.
    /// </summary>
    public async Task<IEmbeddingGenerator<string, Embedding<float>>> GetOrCreateEmbeddingGeneratorAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Embeddings);

        var workspace = await GetWorkspaceAsync(workspaceName, cancellationToken);
        var embedderConfiguration = await _embedderResolver.ResolveAsync(workspace, cancellationToken);
        var stamp = await GetEmbedderStampAsync(workspace.Name, cancellationToken);
        var cacheKey = BuildEmbeddingCacheKey(workspace.Id, embedderConfiguration.ConfigurationId);

        if (TryGetValidGenerator(cacheKey, stamp, out var cached))
        {
            _logger.LogDebug(
                "Using cached EmbeddingGenerator for workspace {WorkspaceName} (WorkspaceId={WorkspaceId}, ConfigurationId={ConfigurationId})",
                workspaceName,
                workspace.Id,
                embedderConfiguration.ConfigurationId);
            return cached;
        }

        await EmbeddingConstructionLock.WaitAsync(cancellationToken);
        try
        {
            stamp = await GetEmbedderStampAsync(workspace.Name, cancellationToken);
            if (TryGetValidGenerator(cacheKey, stamp, out cached))
            {
                return cached;
            }

            _logger.LogInformation(
                "Creating EmbeddingGenerator for workspace {WorkspaceName} (WorkspaceId={WorkspaceId}, ConfigurationId={ConfigurationId}, Provider: {Provider}, Model: {Model})",
                workspaceName,
                workspace.Id,
                embedderConfiguration.ConfigurationId,
                workspace.Provider,
                embedderConfiguration.Model);

            var embeddingGenerator = WorkspaceConfigurationHelper.CreateEmbeddingGenerator(workspace, embedderConfiguration);
            ReplaceGenerator(cacheKey, new CachedEmbeddingGenerator
            {
                Generator = embeddingGenerator,
                Stamp = stamp,
                WorkspaceName = workspace.Name
            });
            return embeddingGenerator;
        }
        finally
        {
            EmbeddingConstructionLock.Release();
        }
    }

    /// <summary>
    /// Invalidates cached embedding generators for a workspace across processes.
    /// </summary>
    public async Task ClearWorkspaceCache(string workspaceName)
    {
        await _embedderStampCache.SetAsync(
            workspaceName,
            new WorkspaceEmbedderCacheStamp { Stamp = Guid.NewGuid().ToString("N") });

        await _providerModelStampCache.SetAsync(
            workspaceName,
            new WorkspaceProviderModelCacheStamp { Stamp = Guid.NewGuid().ToString("N") });

        RemoveLocalGenerators(workspaceName);
        _logger.LogInformation("Cleared embedder and provider model-list cache for workspace {WorkspaceName}", workspaceName);
    }

    public async Task<string> GetProviderModelStampAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        var item = await _providerModelStampCache.GetAsync(workspaceName, token: cancellationToken);
        if (item != null && !string.IsNullOrWhiteSpace(item.Stamp))
        {
            return item.Stamp;
        }

        var stamp = Guid.NewGuid().ToString("N");
        await _providerModelStampCache.SetAsync(
            workspaceName,
            new WorkspaceProviderModelCacheStamp { Stamp = stamp },
            token: cancellationToken);
        return stamp;
    }

    private async Task<string> GetEmbedderStampAsync(string workspaceName, CancellationToken cancellationToken)
    {
        var item = await _embedderStampCache.GetAsync(workspaceName, token: cancellationToken);
        if (item != null && !string.IsNullOrWhiteSpace(item.Stamp))
        {
            return item.Stamp;
        }

        var stamp = Guid.NewGuid().ToString("N");
        await _embedderStampCache.SetAsync(
            workspaceName,
            new WorkspaceEmbedderCacheStamp { Stamp = stamp },
            token: cancellationToken);
        return stamp;
    }

    private string BuildEmbeddingCacheKey(Guid workspaceId, Guid configurationId)
    {
        var tenantKey = _currentTenant.Id?.ToString("N") ?? "host";
        return $"{tenantKey}:{workspaceId:N}:{configurationId:N}";
    }

    private static bool TryGetValidGenerator(
        string cacheKey,
        string stamp,
        out IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        if (EmbeddingGenerators.TryGetValue(cacheKey, out var cached) &&
            string.Equals(cached.Stamp, stamp, StringComparison.Ordinal))
        {
            generator = cached.Generator;
            return true;
        }

        generator = null!;
        return false;
    }

    private static void ReplaceGenerator(string cacheKey, CachedEmbeddingGenerator next)
    {
        if (EmbeddingGenerators.TryRemove(cacheKey, out var previous))
        {
            DisposeGenerator(previous.Generator);
        }

        EmbeddingGenerators[cacheKey] = next;
    }

    private void RemoveLocalGenerators(string workspaceName)
    {
        var tenantPrefix = (_currentTenant.Id?.ToString("N") ?? "host") + ":";
        foreach (var pair in EmbeddingGenerators)
        {
            if (!pair.Key.StartsWith(tenantPrefix, StringComparison.Ordinal) ||
                !string.Equals(pair.Value.WorkspaceName, workspaceName, StringComparison.Ordinal))
            {
                continue;
            }

            if (EmbeddingGenerators.TryRemove(pair.Key, out var removed))
            {
                DisposeGenerator(removed.Generator);
            }
        }
    }

    private static void DisposeGenerator(IEmbeddingGenerator<string, Embedding<float>> generator)
    {
        (generator as IDisposable)?.Dispose();
    }

    private async Task<Workspace> GetWorkspaceAsync(string workspaceName, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);

        if (workspace == null)
        {
            _logger.LogDebug("WorkspaceSyncService could not find workspace {WorkspaceName}", workspaceName);
            throw new Volo.Abp.BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        if (!workspace.IsActive)
        {
            _logger.LogDebug(
                "WorkspaceSyncService found inactive workspace {WorkspaceName} with id {WorkspaceId}",
                workspaceName,
                workspace.Id);
            throw new Volo.Abp.BusinessException(AIErrorCodes.WorkspaceNotActive)
                .WithData("WorkspaceName", workspaceName);
        }

        _logger.LogDebug(
            "WorkspaceSyncService resolved workspace {WorkspaceName}. WorkspaceId={WorkspaceId}, Provider={Provider}, Model={Model}, ApiBaseUrlConfigured={ApiBaseUrlConfigured}, ApiKeyConfigured={ApiKeyConfigured}",
            workspaceName,
            workspace.Id,
            workspace.Provider,
            workspace.Model,
            !string.IsNullOrWhiteSpace(workspace.ApiBaseUrl),
            !string.IsNullOrWhiteSpace(workspace.ApiKey));

        return workspace;
    }

    private async Task CheckFeatureAsync(string featureName)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAIFeatures.Enable))
        {
            throw new Volo.Abp.BusinessException($"Feature is disabled: {SufiAIFeatures.Enable}");
        }

        if (!await _featureChecker.IsEnabledAsync(featureName))
        {
            throw new Volo.Abp.BusinessException($"Feature is disabled: {featureName}");
        }
    }

    private sealed class CachedEmbeddingGenerator
    {
        public required IEmbeddingGenerator<string, Embedding<float>> Generator { get; init; }
        public required string Stamp { get; init; }
        public required string WorkspaceName { get; init; }
    }
}
