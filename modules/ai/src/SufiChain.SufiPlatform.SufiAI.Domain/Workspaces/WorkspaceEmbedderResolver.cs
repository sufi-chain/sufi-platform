using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceEmbedderResolver : IWorkspaceEmbedderResolver, ITransientDependency
{
    protected IAIModelConfigurationRepository ConfigurationRepository { get; }
    protected IAICredentialResolver CredentialResolver { get; }

    public WorkspaceEmbedderResolver(
        IAIModelConfigurationRepository configurationRepository,
        IAICredentialResolver credentialResolver)
    {
        ConfigurationRepository = configurationRepository;
        CredentialResolver = credentialResolver;
    }

    public virtual async Task<EmbedderConfiguration> ResolveAsync(
        Workspace workspace,
        CancellationToken cancellationToken = default)
    {
        var configuration = await ConfigurationRepository.GetPrimaryConfigurationAsync(
            workspace.Id,
            AICapabilityType.Embeddings,
            cancellationToken);

        if (configuration == null)
        {
            throw new BusinessException(AIErrorCodes.EmbeddingsModelNotConfigured)
                .WithData("WorkspaceName", workspace.Name);
        }

        var apiKey = CredentialResolver.DecryptApiKey(configuration.ApiKey)
            ?? CredentialResolver.DecryptApiKey(workspace.ApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new BusinessException(AIErrorCodes.EmbeddingsCredentialsMissing)
                .WithData("WorkspaceName", workspace.Name);
        }

        var apiBaseUrl = !string.IsNullOrWhiteSpace(configuration.ApiEndpoint)
            ? configuration.ApiEndpoint
            : workspace.ApiBaseUrl;

        return new EmbedderConfiguration
        {
            ConfigurationId = configuration.Id,
            Provider = workspace.Provider,
            Model = configuration.ModelId,
            ApiKey = apiKey,
            ApiBaseUrl = apiBaseUrl,
            Dimensions = configuration.Dimensions
                ?? EmbeddingModelDefaults.GetDimensions(configuration.ModelId),
            MaxInputTokens = EmbeddingModelDefaults.GetMaxInputTokens(configuration.ModelId),
            EncodingFormat = EmbeddingModelDefaults.GetEncodingFormat(configuration.ModelId, apiBaseUrl),
            SupportsDimensionsParameter = !EmbeddingModelDefaults.RequiresCustomTransport(
                configuration.ModelId,
                apiBaseUrl)
        };
    }
}
