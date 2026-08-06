using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceEmbedderResolver : IWorkspaceEmbedderResolver, ITransientDependency
{
    protected IAIModelConfigurationRepository ConfigurationRepository { get; }
    protected IStringEncryptionService StringEncryptor { get; }

    public WorkspaceEmbedderResolver(
        IAIModelConfigurationRepository configurationRepository,
        IStringEncryptionService stringEncryptor)
    {
        ConfigurationRepository = configurationRepository;
        StringEncryptor = stringEncryptor;
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

        var apiKey = DecryptApiKey(configuration.ApiKey) ?? DecryptApiKey(workspace.ApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new BusinessException(AIErrorCodes.EmbeddingsCredentialsMissing)
                .WithData("WorkspaceName", workspace.Name);
        }

        return new EmbedderConfiguration
        {
            Provider = workspace.Provider,
            Model = configuration.ModelId,
            ApiKey = apiKey,
          ApiBaseUrl = !string.IsNullOrWhiteSpace(configuration.ApiEndpoint)
              ? configuration.ApiEndpoint
               : workspace.ApiBaseUrl,
           Dimensions = configuration.Dimensions
               ?? EmbeddingModelDefaults.GetDimensions(configuration.ModelId)
      };
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
