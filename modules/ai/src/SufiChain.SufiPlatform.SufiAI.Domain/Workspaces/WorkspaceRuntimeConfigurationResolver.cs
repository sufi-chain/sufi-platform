using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceRuntimeConfigurationResolver : DomainService, IWorkspaceRuntimeConfigurationResolver
{
    protected IWorkspaceRepository WorkspaceRepository { get; }

    protected IReadOnlyList<IAIProvider> Providers { get; }

    protected IReadOnlyList<IAiProviderProfile> Profiles { get; }

    protected IAICredentialResolver CredentialResolver { get; }

    protected IAIModelRouteResolver RouteResolver =>
        LazyServiceProvider.LazyGetRequiredService<IAIModelRouteResolver>();

    public WorkspaceRuntimeConfigurationResolver(
        IWorkspaceRepository workspaceRepository,
        IEnumerable<IAIProvider> providers,
        IEnumerable<IAiProviderProfile> profiles,
        IAICredentialResolver credentialResolver)
    {
        WorkspaceRepository = workspaceRepository;
        Providers = providers.ToList();
        Profiles = profiles.ToList();
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
        var profile = AiProviderProfiles.Find(Profiles, workspace.Provider);
        var provider = ResolveExecutor(workspace.Provider, profile);
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
        if (failureCode == null &&
            profile != null &&
            capabilityType == AICapabilityType.ChatCompletion &&
            !profile.SupportsApiMode(configuration?.OpenAIApiMode ?? OpenAIApiMode.ChatCompletions))
        {
            failureCode = WorkspaceRuntimeFailureCodes.CapabilityNotSupported;
        }

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
            InputPrice = FirstPrice(
                configuration?.InputPrice,
                workspace.InputCostPer1MTokens,
                configuration == null || configuration.InputPriceUnit == AIPriceUnit.PerMillionTokens),
            InputPriceUnit = configuration?.InputPriceUnit ?? AIPriceUnit.PerMillionTokens,
            OutputPrice = FirstPrice(
                configuration?.OutputPrice,
                workspace.OutputCostPer1MTokens,
                configuration == null ||
                (configuration.InputPriceUnit == AIPriceUnit.PerMillionTokens &&
                 configuration.OutputPriceUnit == AIPriceUnit.PerMillionTokens)),
            OutputPriceUnit = configuration?.OutputPriceUnit ?? AIPriceUnit.PerMillionTokens,
            IsFallback = configuration == null && isConfigured,
            ModelConfigurationId = configuration?.Id,
            IsExplicitSelection = isExplicitSelection,
            IsConfigured = isConfigured,
            IsReady = failureCode == null,
            FailureCode = failureCode
        };
    }

    /// <summary>
    /// Workspace prices are a token fallback. A minute, hour, character, image, or request route does not inherit them.
    /// </summary>
    private static decimal? FirstPrice(decimal? routePrice, decimal? workspaceTokenPrice, bool inheritWorkspace)
    {
        if (routePrice.HasValue)
        {
            return routePrice;
        }

        return inheritWorkspace ? workspaceTokenPrice : null;
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
        if (!AiProviderProfiles.IsOpenAICompatible(Profiles, configuration.Provider) ||
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

        var profile = AiProviderProfiles.Find(Profiles, workspace.Provider);
        if (profile != null && !profile.SupportsCapability(capabilityType))
        {
            return WorkspaceRuntimeFailureCodes.CapabilityNotSupported;
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

    private IAIProvider? ResolveExecutor(AIProviderType providerType, IAiProviderProfile? profile)
    {
        if (profile != null)
        {
            return Providers.FirstOrDefault(item => item.CapabilityKind == profile.CapabilityKind);
        }

        return Providers.FirstOrDefault(item => item.ProviderType == providerType);
    }
}
