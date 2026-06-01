using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Blazor.Public.Services;

/// <summary>
/// Default token provider for anonymous or host-unconfigured scenarios.
/// </summary>
public class NullChatHubConnectionAccessTokenProvider : IChatHubConnectionAccessTokenProvider, ITransientDependency
{
    public Task<string?> GetAccessTokenAsync()
    {
        return Task.FromResult<string?>(null);
    }
}
