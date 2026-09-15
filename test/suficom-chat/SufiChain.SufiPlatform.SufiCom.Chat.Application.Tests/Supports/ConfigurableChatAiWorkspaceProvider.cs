using SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Supports;

public class ConfigurableChatAiWorkspaceProvider : IChatAiWorkspaceProvider, ISingletonDependency
{
    public bool IntegrationReady { get; set; }

    public HashSet<string> HealthyWorkspaces { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<Guid> HealthyWorkspaceIds { get; } = [];

    public Task<bool> IsIntegrationReadyAsync()
    {
        return Task.FromResult(IntegrationReady);
    }

    public Task<bool> IsHealthyAsync(string workspaceName)
    {
        return Task.FromResult(HealthyWorkspaces.Contains(workspaceName));
    }

    public Task<bool> IsHealthyAsync(Guid workspaceId)
    {
        return Task.FromResult(HealthyWorkspaceIds.Contains(workspaceId));
    }
}
