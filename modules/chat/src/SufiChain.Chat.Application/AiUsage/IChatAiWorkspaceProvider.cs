using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.AiUsage;

public interface IChatAiWorkspaceProvider
{
    Task<bool> IsIntegrationReadyAsync();

    Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync();

    Task<bool> IsHealthyAsync(string workspaceName);
}

public class NullChatAiWorkspaceProvider : IChatAiWorkspaceProvider, ITransientDependency
{
    public virtual Task<bool> IsIntegrationReadyAsync()
    {
        return Task.FromResult(false);
    }

    public virtual Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync()
    {
        return Task.FromResult(new List<ChatAiWorkspaceOptionDto>());
    }

    public virtual Task<bool> IsHealthyAsync(string workspaceName)
    {
        return Task.FromResult(false);
    }
}
