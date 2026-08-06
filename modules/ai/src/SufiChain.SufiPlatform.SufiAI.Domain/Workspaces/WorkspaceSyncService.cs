using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;
using SufiChain.SufiPlatform.Features;
using System.Collections.Concurrent;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Synchronizes workspace configuration from database to Sufi AI framework.
/// Creates and caches ChatClient, Kernel, and EmbeddingGenerator instances on-demand per workspace.
/// </summary>
public class WorkspaceSyncService : ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceEmbedderResolver _embedderResolver;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureChecker _featureChecker;
    private readonly IStringEncryptionService _stringEncryptor;
    private readonly ILogger<WorkspaceSyncService> _logger;
    
    // Cache for workspace instances
    private static readonly ConcurrentDictionary<string, IChatClient> _chatClientCache = new();
    private static readonly ConcurrentDictionary<string, Kernel> _kernelCache = new();
    private static readonly ConcurrentDictionary<string, IEmbeddingGenerator<string, Embedding<float>>> _embeddingGeneratorCache = new();

    public WorkspaceSyncService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceEmbedderResolver embedderResolver,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        IServiceProvider serviceProvider,
        IFeatureChecker featureChecker,
        IStringEncryptionService stringEncryptor,
        ILogger<WorkspaceSyncService> logger)
    {
        _workspaceRepository = workspaceRepository;
        _embedderResolver = embedderResolver;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _serviceProvider = serviceProvider;
        _featureChecker = featureChecker;
        _stringEncryptor = stringEncryptor;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a ChatClient for the workspace.
    /// </summary>
    public async Task<IChatClient> GetOrCreateChatClientAsync(string workspaceName, CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Chat);

        if (_chatClientCache.TryGetValue(workspaceName, out var cachedClient))
        {
            _logger.LogDebug("Using cached ChatClient for workspace {WorkspaceName}", workspaceName);
            return cachedClient;
        }

        var workspace = await GetWorkspaceAsync(workspaceName, cancellationToken);
        
        _logger.LogInformation("Creating ChatClient for workspace {WorkspaceName} (Provider: {Provider}, Model: {Model})",
            workspaceName, workspace.Provider, workspace.Model);

        var builder = new ChatClientBuilder(_serviceProvider.GetService<IChatClient>());
        WorkspaceConfigurationHelper.ConfigureChatClient(builder, workspace);
        var chatClient = builder.Build(_serviceProvider);

        _chatClientCache.TryAdd(workspaceName, chatClient);
        return chatClient;
    }

    /// <summary>
    /// Gets or creates a Kernel for the workspace.
    /// </summary>
    public async Task<Kernel> GetOrCreateKernelAsync(string workspaceName, CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Workspaces);

        if (_kernelCache.TryGetValue(workspaceName, out var cachedKernel))
        {
            _logger.LogDebug("Using cached Kernel for workspace {WorkspaceName}", workspaceName);
            return cachedKernel;
        }

        var configuration = await _runtimeConfigurationResolver.ResolveAsync(
            workspaceName,
            AICapabilityType.ChatCompletion,
            cancellationToken);
        
        _logger.LogInformation("Creating Kernel for workspace {WorkspaceName} (Provider: {Provider}, Model: {Model})",
            workspaceName, configuration.Provider, configuration.ModelId);

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_serviceProvider);
        WorkspaceConfigurationHelper.ConfigureKernel(builder, configuration);
        var kernel = builder.Build();

        _kernelCache.TryAdd(workspaceName, kernel);
        return kernel;
    }

    public async Task<Kernel> CreateRequestKernelAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Workspaces);
        var configuration = await _runtimeConfigurationResolver.ResolveAsync(
            workspaceName,
            AICapabilityType.ChatCompletion,
            cancellationToken);
        return await CreateRequestKernelAsync(configuration, cancellationToken);
    }

    public async Task<Kernel> CreateRequestKernelAsync(
        WorkspaceRuntimeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Workspaces);
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_serviceProvider);
        WorkspaceConfigurationHelper.ConfigureKernel(builder, configuration);
        return builder.Build();
    }

    /// <summary>
    /// Gets or creates an EmbeddingGenerator for the workspace.
    /// </summary>
    public async Task<IEmbeddingGenerator<string, Embedding<float>>> GetOrCreateEmbeddingGeneratorAsync(
        string workspaceName, 
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync(SufiAIFeatures.Embeddings);

        if (_embeddingGeneratorCache.TryGetValue(workspaceName, out var cachedGenerator))
        {
            _logger.LogDebug("Using cached EmbeddingGenerator for workspace {WorkspaceName}", workspaceName);
            return cachedGenerator;
        }

        var workspace = await GetWorkspaceAsync(workspaceName, cancellationToken);
        var embedderConfiguration = await _embedderResolver.ResolveAsync(workspace, cancellationToken);

        _logger.LogInformation(
            "Creating EmbeddingGenerator for workspace {WorkspaceName} (Provider: {Provider}, Model: {Model})",
            workspaceName,
            workspace.Provider,
            embedderConfiguration.Model);

        var embeddingGenerator = WorkspaceConfigurationHelper.CreateEmbeddingGenerator(workspace, embedderConfiguration);

        _embeddingGeneratorCache.TryAdd(workspaceName, embeddingGenerator);
        return embeddingGenerator;
    }

    /// <summary>
    /// Clears cached instances for a workspace (e.g., after configuration changes).
    /// </summary>
    public void ClearWorkspaceCache(string workspaceName)
    {
        _chatClientCache.TryRemove(workspaceName, out _);
        _kernelCache.TryRemove(workspaceName, out _);
        _embeddingGeneratorCache.TryRemove(workspaceName, out _);
        _logger.LogInformation("Cleared cache for workspace {WorkspaceName}", workspaceName);
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

    private string? DecryptApiKey(string? encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return encryptedApiKey;
        }

        try
        {
            return _stringEncryptor.Decrypt(encryptedApiKey);
        }
        catch
        {
            return encryptedApiKey;
        }
    }

}
