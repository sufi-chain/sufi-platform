using Volo.Abp;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceRuntimeConfigurationResolver : IWorkspaceRuntimeConfigurationResolver
{
    protected IWorkspaceRepository WorkspaceRepository { get; }

    protected IReadOnlyList<IAIProvider> Providers { get; }

    protected IStringEncryptionService StringEncryptor { get; }

    public WorkspaceRuntimeConfigurationResolver(
        IWorkspaceRepository workspaceRepository,
        IEnumerable<IAIProvider> providers,
        IStringEncryptionService stringEncryptor)
    {
        WorkspaceRepository = workspaceRepository;
        Providers = providers.ToList();
        StringEncryptor = stringEncryptor;
    }

    public virtual async Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var workspace = await WorkspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        return Resolve(workspace, capabilityType);
    }

    public virtual async Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var workspace = await WorkspaceRepository.FindAsync(
            workspaceId,
            includeDetails: true,
            cancellationToken: cancellationToken);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceId", workspaceId);
        }

        return Resolve(workspace, capabilityType);
    }

    public virtual WorkspaceRuntimeConfiguration Resolve(
        Workspace workspace,
        AICapabilityType capabilityType)
    {
        var configuration = workspace.GetPrimaryConfiguration(capabilityType);
        var fallbackModel = capabilityType == AICapabilityType.ChatCompletion
            ? workspace.DefaultModel
            : null;
        var modelId = configuration?.ModelId ?? fallbackModel ?? string.Empty;
        var isConfigured = !string.IsNullOrWhiteSpace(modelId);
        var provider = Providers.FirstOrDefault(item => item.ProviderType == workspace.Provider);
        var effectiveApiKey = DecryptApiKey(configuration?.ApiKey ?? workspace.ApiKey);
        var failureCode = GetFailureCode(
            workspace,
            capabilityType,
            provider,
            isConfigured,
            configuration?.ApiEndpoint ?? workspace.ApiBaseUrl,
            effectiveApiKey);

        return new WorkspaceRuntimeConfiguration
        {
            Workspace = workspace,
            ModelConfiguration = configuration,
            CapabilityType = capabilityType,
            Provider = workspace.Provider,
            ModelId = modelId,
            ApiEndpoint = configuration?.ApiEndpoint ?? workspace.ApiBaseUrl,
            ApiKey = effectiveApiKey,
            OpenAIApiMode = configuration?.OpenAIApiMode ?? workspace.OpenAIApiMode,
            InputCostPer1MTokens = configuration?.InputCostPer1MTokens ?? workspace.InputCostPer1MTokens,
            OutputCostPer1MTokens = configuration?.OutputCostPer1MTokens ?? workspace.OutputCostPer1MTokens,
            IsFallback = configuration == null && isConfigured,
            IsConfigured = isConfigured,
            IsReady = failureCode == null,
            FailureCode = failureCode
        };
    }

    protected virtual string? GetFailureCode(
        Workspace workspace,
        AICapabilityType capabilityType,
        IAIProvider? provider,
        bool isConfigured,
        string? effectiveApiEndpoint,
        string? effectiveApiKey)
    {
        if (!workspace.IsActive)
        {
            return WorkspaceRuntimeFailureCodes.WorkspaceInactive;
        }

        if (!isConfigured)
        {
            return WorkspaceRuntimeFailureCodes.ModelNotConfigured;
        }

        if (provider == null)
        {
            return WorkspaceRuntimeFailureCodes.ProviderNotRegistered;
        }

        if (!provider.SupportsCapability(capabilityType))
        {
            return WorkspaceRuntimeFailureCodes.CapabilityNotSupported;
        }

        if (!string.IsNullOrWhiteSpace(effectiveApiEndpoint) &&
            (!Uri.TryCreate(effectiveApiEndpoint, UriKind.Absolute, out var endpointUri) ||
             (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps)))
        {
            return WorkspaceRuntimeFailureCodes.EndpointInvalid;
        }

        return string.IsNullOrWhiteSpace(effectiveApiKey)
            ? WorkspaceRuntimeFailureCodes.CredentialsMissing
            : null;
    }

    protected virtual string? DecryptApiKey(string? encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return encryptedApiKey;
        }

        try
        {
            return StringEncryptor.Decrypt(encryptedApiKey);
        }
        catch
        {
            return encryptedApiKey;
        }
    }
}
