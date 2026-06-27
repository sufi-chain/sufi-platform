using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AI.RAG;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;
using SufiChain.SufiAbp.Features;
using System.Collections.Concurrent;

namespace SufiChain.SufiAbp.AI.Workspaces;

/// <summary>
/// Synchronizes workspace configuration from database to SufiAbp AI framework.
/// Creates and caches ChatClient, Kernel, and EmbeddingGenerator instances on-demand per workspace.
/// </summary>
public class WorkspaceSyncService : ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
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
        IServiceProvider serviceProvider,
        IFeatureChecker featureChecker,
        IStringEncryptionService stringEncryptor,
        ILogger<WorkspaceSyncService> logger)
    {
        _workspaceRepository = workspaceRepository;
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
            return cachedClient;
        }

        var workspace = await GetWorkspaceAsync(workspaceName, cancellationToken);
        
        _logger.LogInformation("Creating ChatClient for workspace {WorkspaceName} (Provider: {Provider}, Model: {Model})",
            workspaceName, workspace.Provider, workspace.Model);

        //ToDo
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
            return cachedKernel;
        }

        var workspace = await GetWorkspaceAsync(workspaceName, cancellationToken);
        
        _logger.LogInformation("Creating Kernel for workspace {WorkspaceName} (Provider: {Provider}, Model: {Model})",
            workspaceName, workspace.Provider, workspace.Model);

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_serviceProvider);
        WorkspaceConfigurationHelper.ConfigureKernel(builder, workspace, DecryptApiKey(workspace.ApiKey));
        var kernel = builder.Build();

        _kernelCache.TryAdd(workspaceName, kernel);
        return kernel;
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
            return cachedGenerator;
        }

        var workspace = await GetWorkspaceAsync(workspaceName, cancellationToken);
        
        _logger.LogInformation("Creating EmbeddingGenerator for workspace {WorkspaceName} (Provider: {Provider})",
            workspaceName, workspace.Provider);

        var embeddingGenerator = WorkspaceConfigurationHelper.CreateEmbeddingGenerator(workspace, ResolveEmbedderConfiguration(workspace));

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
            throw new Volo.Abp.BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        if (!workspace.IsActive)
        {
            throw new Volo.Abp.BusinessException(AIErrorCodes.WorkspaceNotActive)
                .WithData("WorkspaceName", workspaceName);
        }

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

    private EmbedderConfiguration? ResolveEmbedderConfiguration(Workspace workspace)
    {
        var config = WorkspaceConfigurationHelper.ParseEmbedderConfig(workspace);
        if (config == null)
        {
            return null;
        }

        config.ApiKey = DecryptApiKey(config.ApiKey);
        config.ApiBaseUrl ??= workspace.ApiBaseUrl;
        config.Model = string.IsNullOrWhiteSpace(config.Model) ? workspace.DefaultModel : config.Model;
        return config;
    }
}
