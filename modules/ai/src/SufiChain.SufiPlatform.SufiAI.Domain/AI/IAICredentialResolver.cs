using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAICredentialResolver : ITransientDependency
{
    string? DecryptApiKey(string? encryptedApiKey);
}
