using Volo.Abp.Application.Services;

namespace SufiChain.Chat.AiUsage;

public interface IChatAssistantConfigurationAppService : IApplicationService
{
    Task<ChatAssistantConfigurationDto> GetAsync();

    Task UpdateAsync(UpdateChatAssistantConfigurationInput input);
}

public class ChatAssistantConfigurationDto
{
    public bool IsAvailable { get; set; }

    public string? MessageKey { get; set; }

    public string? DefaultWorkspaceName { get; set; }

    public List<ChatAssistantMappingDto> Mappings { get; set; } = new();

    public List<ChatAiWorkspaceOptionDto> WorkspaceOptions { get; set; } = new();
}

public class ChatAssistantMappingDto
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string WorkspaceName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public bool IsPublic { get; set; } = true;

    public bool IsWorkspaceHealthy { get; set; }
}

public class UpdateChatAssistantConfigurationInput
{
    public string? DefaultWorkspaceName { get; set; }

    public List<ChatAssistantMappingDto> Mappings { get; set; } = new();
}
