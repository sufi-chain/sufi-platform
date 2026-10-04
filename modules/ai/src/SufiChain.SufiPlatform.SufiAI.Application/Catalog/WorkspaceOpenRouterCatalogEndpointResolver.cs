using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public class WorkspaceOpenRouterCatalogEndpointResolver : IOpenRouterCatalogEndpointResolver, ITransientDependency
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IStringEncryptionService _stringEncryptor;
    private readonly ILogger<WorkspaceOpenRouterCatalogEndpointResolver> _logger;

    public WorkspaceOpenRouterCatalogEndpointResolver(
        IWorkspaceRepository workspaces,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        IStringEncryptionService stringEncryptor,
        ILogger<WorkspaceOpenRouterCatalogEndpointResolver> logger)
    {
        _workspaces = workspaces;
        _currentTenant = currentTenant;
        _unitOfWorkManager = unitOfWorkManager;
        _stringEncryptor = stringEncryptor;
        _logger = logger;
    }

    public virtual async Task<OpenRouterCatalogEndpoint> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var workspace = await FindWorkspaceAsync(cancellationToken);
        if (workspace == null || string.IsNullOrWhiteSpace(workspace.ApiBaseUrl))
        {
            return new OpenRouterCatalogEndpoint();
        }

        return new OpenRouterCatalogEndpoint
        {
            BaseUrl = workspace.ApiBaseUrl.Trim(),
            ApiKey = Decrypt(workspace.ApiKey)
        };
    }

    protected virtual async Task<Workspace?> FindWorkspaceAsync(CancellationToken cancellationToken)
    {
        var current = Select(await ListAsync(cancellationToken));
        if (current != null || _currentTenant.Id == null)
        {
            return current;
        }

        using (_currentTenant.Change(null))
        {
            using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            var host = Select(await ListAsync(cancellationToken));
            await unitOfWork.CompleteAsync(cancellationToken);
            return host;
        }
    }

    protected virtual async Task<List<Workspace>> ListAsync(CancellationToken cancellationToken)
    {
        var count = await _workspaces.GetCountAsync(cancellationToken: cancellationToken);
        if (count <= 0)
        {
            return new List<Workspace>();
        }

        var max = count > int.MaxValue ? int.MaxValue : (int)count;
        return await _workspaces.GetListAsync(
            skipCount: 0,
            maxResultCount: max,
            sorting: nameof(Workspace.Name),
            cancellationToken: cancellationToken);
    }

    protected virtual Workspace? Select(IReadOnlyList<Workspace> workspaces)
    {
        var candidates = workspaces
            .Where(workspace =>
                workspace.Provider == AIProviderType.OpenRouter &&
                workspace.IsActive &&
                !workspace.IsInherited &&
                !string.IsNullOrWhiteSpace(workspace.ApiBaseUrl))
            .ToList();

        var selected = HostDefaultWorkspaceMarker.Select(candidates, configuredName: null)
                       ?? candidates.FirstOrDefault();
        if (selected == null)
        {
            _logger.LogDebug("No OpenRouter workspace with an API base URL is available for the model list.");
        }

        return selected;
    }

    private string? Decrypt(string? encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return null;
        }

        try
        {
            var decrypted = _stringEncryptor.Decrypt(encryptedApiKey);
            return string.IsNullOrWhiteSpace(decrypted) ? null : decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenRouter workspace API key could not be decrypted for the model list.");
            return null;
        }
    }
}
