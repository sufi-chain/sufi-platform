using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceRuntimeConfigurationResolver : DomainService, IWorkspaceRuntimeConfigurationResolver
{
    protected IWorkspaceRepository WorkspaceRepository { get; }

    protected IReadOnlyList<IAIProvider> Providers { get; }

    protected IAICredentialResolver CredentialResolver { get; }

    protected IAIModelRouteResolver RouteResolver =>
        LazyServiceProvider.LazyGetRequiredService<IAIModelRouteResolver>();

    public WorkspaceRuntimeConfigurationResolver(
        IWorkspaceRepository workspaceRepository,
        IEnumerable<IAIProvider> providers,
        IAICredentialResolver credentialResolver)
    {
        WorkspaceRepository = workspaceRepository;
        Providers = providers.ToList();
        CredentialResolver = credentialResolver;
    }

    public virtual Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        return ResolveAsync(
            workspaceName,
            capabilityType,
            AIModelRouteSelection.Implicit,
            cancellationToken);
    }

    public virtual async Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken = default)
    {
        var workspace = await WorkspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        return RouteResolver.Resolve(workspace, capabilityType, selection);
    }

    public virtual Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        return ResolveAsync(
            workspaceId,
            capabilityType,
            AIModelRouteSelection.Implicit,
            cancellationToken);
    }

    public virtual Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken = default)
    {
        return RouteResolver.ResolveAsync(
            workspaceId,
            capabilityType,
            selection,
            cancellationToken);
    }

    public virtual WorkspaceRuntimeConfiguration Resolve(
        Workspace workspace,
        AICapabilityType capabilityType,
        AIModelConfiguration? configuration = null,
        bool isExplicitSelection = false)
    {
        configuration ??= workspace.GetPrimaryConfiguration(capabilityType);
        var fallbackModel = capabilityType == AICapabilityType.ChatCompletion
            ? workspace.DefaultModel
            : null;
        var modelId = configuration?.ModelId ?? fallbackModel ?? string.Empty;
        var isConfigured = !string.IsNullOrWhiteSpace(modelId);
        var provider = Providers.FirstOrDefault(item => item.ProviderType == workspace.Provider);
        var effectiveApiKey = CredentialResolver.DecryptApiKey(configuration?.ApiKey)
            ?? CredentialResolver.DecryptApiKey(workspace.ApiKey);
        var effectiveApiEndpoint = configuration?.ApiEndpoint ?? workspace.ApiBaseUrl;
        var failureCode = GetFailureCode(
            workspace,
            capabilityType,
            provider,
            isConfigured,
            effectiveApiEndpoint,
            effectiveApiKey);

        return new WorkspaceRuntimeConfiguration
        {
            Workspace = workspace,
            ModelConfiguration = configuration,
            CapabilityType = capabilityType,
            Provider = workspace.Provider,
            ModelId = modelId,
            ApiEndpoint = effectiveApiEndpoint,
            ApiKey = effectiveApiKey,
            OpenAIApiMode = configuration?.OpenAIApiMode ?? OpenAIApiMode.ChatCompletions,
            MaxContextTokens = configuration?.MaxContextTokens > 0
                ? configuration.MaxContextTokens
                : AIModelConfiguration.DefaultMaxContextTokens,
            InputCostPer1MTokens = configuration?.InputCostPer1MTokens ?? workspace.InputCostPer1MTokens,
            OutputCostPer1MTokens = configuration?.OutputCostPer1MTokens ?? workspace.OutputCostPer1MTokens,
            IsFallback = configuration == null && isConfigured,
            ModelConfigurationId = configuration?.Id,
            IsExplicitSelection = isExplicitSelection,
            IsConfigured = isConfigured,
            IsReady = failureCode == null,
            FailureCode = failureCode
        };
    }

    public virtual void EnsureReady(
        WorkspaceRuntimeConfiguration configuration,
        bool requiresToolCalling = false)
    {
        if (requiresToolCalling)
        {
            EnsureToolCallingCompatible(configuration);
            return;
        }

        ThrowIfNotReady(configuration);
    }

    protected virtual void EnsureToolCallingCompatible(WorkspaceRuntimeConfiguration configuration)
    {
        if (configuration.Provider != AIProviderType.OpenAI ||
            string.Equals(
                configuration.FailureCode,
                WorkspaceRuntimeFailureCodes.ProviderNotRegistered,
                StringComparison.Ordinal))
        {
            throw CreateToolCallingException(
                    AIErrorCodes.McpProviderNotSupported,
                    configuration)
                .WithData("ModelId", configuration.ModelId);
        }

        if (configuration.OpenAIApiMode != OpenAIApiMode.ChatCompletions)
        {
            throw CreateToolCallingException(
                    AIErrorCodes.McpRequiresChatCompletions,
                    configuration)
                .WithData("ModelId", configuration.ModelId)
                .WithData("ApiMode", configuration.OpenAIApiMode.ToString());
        }

        if (!string.IsNullOrWhiteSpace(configuration.ApiEndpoint) &&
            (!Uri.TryCreate(configuration.ApiEndpoint, UriKind.Absolute, out var endpoint) ||
             (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)))
        {
            throw CreateToolCallingException(
                    AIErrorCodes.McpWorkspaceNotReady,
                    configuration)
                .WithData("FailureCode", WorkspaceRuntimeFailureCodes.EndpointInvalid)
                .WithData("ModelId", configuration.ModelId)
                .WithData("ApiMode", configuration.OpenAIApiMode.ToString());
        }

        if (!configuration.IsReady)
        {
            throw CreateToolCallingException(
                    AIErrorCodes.McpWorkspaceNotReady,
                    configuration)
                .WithData("FailureCode", configuration.FailureCode ?? string.Empty)
                .WithData("ModelId", configuration.ModelId)
                .WithData("ApiMode", configuration.OpenAIApiMode.ToString());
        }
    }

    protected virtual void ThrowIfNotReady(WorkspaceRuntimeConfiguration resolved)
    {
        var exception = resolved.FailureCode switch
        {
            WorkspaceRuntimeFailureCodes.WorkspaceInactive =>
                new BusinessException(AIErrorCodes.WorkspaceNotActive),
            WorkspaceRuntimeFailureCodes.ModelNotConfigured =>
                new BusinessException(AIErrorCodes.NoModelConfigured),
            WorkspaceRuntimeFailureCodes.CredentialsMissing =>
                new BusinessException(AIErrorCodes.ApiKeyRequired),
            WorkspaceRuntimeFailureCodes.ProviderNotRegistered =>
                new BusinessException(AIErrorCodes.ProviderNotSupported),
            WorkspaceRuntimeFailureCodes.CapabilityNotSupported =>
                new BusinessException(AIErrorCodes.CapabilityNotSupported),
            WorkspaceRuntimeFailureCodes.EndpointInvalid =>
                new BusinessException(AIErrorCodes.InvalidProviderConfiguration),
            _ => null
        };

        if (exception == null)
        {
            return;
        }

        throw exception
            .WithData("WorkspaceName", resolved.Workspace.Name)
            .WithData("Provider", resolved.Provider.ToString())
            .WithData("CapabilityType", resolved.CapabilityType.ToString());
    }

    protected virtual BusinessException CreateToolCallingException(
        string errorCode,
        WorkspaceRuntimeConfiguration configuration)
    {
        return new BusinessException(errorCode)
            .WithData("WorkspaceName", configuration.Workspace.Name)
            .WithData("Provider", configuration.Provider.ToString())
            .WithData("CapabilityType", configuration.CapabilityType.ToString());
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
}
