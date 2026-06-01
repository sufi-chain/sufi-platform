using SufiChain.Chat.AiUsage;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Supports;

public class ConfigurableChatAiWorkspaceProvider : IChatAiWorkspaceProvider, ISingletonDependency
{
    public bool IntegrationReady { get; set; }

    public HashSet<string> HealthyWorkspaces { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> IsIntegrationReadyAsync()
    {
        return Task.FromResult(IntegrationReady);
    }

    public Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync()
    {
        return Task.FromResult(HealthyWorkspaces
            .Select(name => new ChatAiWorkspaceOptionDto
            {
                Name = name,
                DisplayName = name,
                IsHealthy = true
            })
            .ToList());
    }

    public Task<bool> IsHealthyAsync(string workspaceName)
    {
        return Task.FromResult(HealthyWorkspaces.Contains(workspaceName));
    }
}
