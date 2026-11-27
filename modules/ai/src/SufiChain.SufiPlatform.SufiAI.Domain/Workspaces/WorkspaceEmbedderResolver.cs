using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceEmbedderResolver : IWorkspaceEmbedderResolver, ITransientDependency
{
    protected IInheritedWorkspaceLiveView LiveView { get; }
    protected IAICredentialResolver CredentialResolver { get; }

    public WorkspaceEmbedderResolver(
        IInheritedWorkspaceLiveView liveView,
        IAICredentialResolver credentialResolver)
    {
        LiveView = liveView;
        CredentialResolver = credentialResolver;
    }

    public virtual async Task<EmbedderConfiguration> ResolveAsync(
        Workspace workspace,
        CancellationToken cancellationToken = default)
    {
        var resolved = workspace.IsInherited && !InheritedWorkspaceLiveView.IsLiveView(workspace)
            ? await LiveView.ApplyAsync(workspace, includeDetails: true, cancellationToken)
            : workspace;
        InheritedWorkspaceLiveView.EnsureUsable(resolved);

        var configuration = resolved.GetPrimaryConfiguration(AICapabilityType.Embeddings);
        if (configuration == null)
        {
            throw new BusinessException(AIErrorCodes.EmbeddingsModelNotConfigured)
                .WithData("WorkspaceName", resolved.Name);
        }

        workspace = resolved;

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
            Dimensions = EmbeddingProviderFailures.ResolveDimensions(
                configuration.ModelId,
                configuration.Dimensions),
            MaxInputTokens = EmbeddingModelDefaults.GetMaxInputTokens(configuration.ModelId),
            EncodingFormat = EmbeddingModelDefaults.GetEncodingFormat(configuration.ModelId, apiBaseUrl),
            SupportsDimensionsParameter = !EmbeddingModelDefaults.RequiresCustomTransport(
                configuration.ModelId,
                apiBaseUrl)
        };
    }
}
