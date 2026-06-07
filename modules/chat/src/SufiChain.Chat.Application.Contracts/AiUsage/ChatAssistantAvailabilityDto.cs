namespace SufiChain.Chat.AiUsage;

public class ChatAssistantAvailabilityDto
{
    public bool IsAvailable { get; set; }

    public string? ReasonCode { get; set; }

    public string? MessageKey { get; set; }

    public List<string> RequiredFeatures { get; set; } = new();

    public List<string> EnabledFeatures { get; set; } = new();

    public string? DefaultWorkspaceName { get; set; }

    public List<ChatAssistantPickerOptionDto> Assistants { get; set; } = new();
}

public class ChatAssistantPickerOptionDto
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string WorkspaceName { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

public class ChatAiWorkspaceSelectionDto
{
    public bool IsAvailable { get; set; }

    public string? ReasonCode { get; set; }

    public string? MessageKey { get; set; }

    public string? DefaultWorkspaceName { get; set; }
}

public class ChatAiWorkspaceOptionDto
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public bool IsHealthy { get; set; }

    public bool IsDefault { get; set; }
}

public class UpdateChatAiWorkspaceSelectionInput
{
    public string? DefaultWorkspaceName { get; set; }
}
