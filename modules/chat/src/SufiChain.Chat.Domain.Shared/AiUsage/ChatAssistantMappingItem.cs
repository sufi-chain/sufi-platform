namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Tenant-defined assistant routing entry stored as JSON in Chat settings.
/// </summary>
public class ChatAssistantMappingItem
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string WorkspaceName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// When true, the assistant is shown in messenger inbox pickers.
    /// Internal-only assistants (operator copilot, KB routing) set this to false.
    /// </summary>
    public bool? IsPublic { get; set; }
}
