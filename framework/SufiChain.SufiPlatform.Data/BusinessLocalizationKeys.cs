namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Canonical key patterns for business-tier localization stored in <c>LocalizationText</c>.
/// </summary>
public static class BusinessLocalizationKeys
{
    public static string FileStructureDisplayName(string structureKey) => $"Structure:{structureKey}:DisplayName";

    public static string FileStructureDescription(string structureKey) => $"Structure:{structureKey}:Description";

    public static string CopilotDisplayName(string copilotKey) => $"Copilot:{copilotKey}:DisplayName";

    public static string CopilotDescription(string copilotKey) => $"Copilot:{copilotKey}:Description";

    public static string CopilotSystemPrompt(string copilotKey) => $"Copilot:{copilotKey}:SystemPrompt";

    public static string CopilotShortcut(string copilotKey, string shortcutId) => $"Copilot:{copilotKey}:Shortcut:{shortcutId}";

    [Obsolete("Use copilot Key overloads. Guid-based keys break tenant-scoped copilots.")]
    public static string CopilotDisplayName(Guid copilotId) => $"Copilot:{copilotId:D}:DisplayName";

    [Obsolete("Use copilot Key overloads.")]
    public static string CopilotDescription(Guid copilotId) => $"Copilot:{copilotId:D}:Description";

    [Obsolete("Use copilot Key overloads.")]
    public static string CopilotSystemPrompt(Guid copilotId) => $"Copilot:{copilotId:D}:SystemPrompt";

    [Obsolete("Use copilot Key overloads.")]
    public static string CopilotShortcut(Guid copilotId, string shortcutId) => $"Copilot:{copilotId:D}:Shortcut:{shortcutId}";

    public static string McpToolDisplayName(string toolName) => $"MCPTool:{toolName}:DisplayName";

    public static string McpToolDescription(string toolName) => $"MCPTool:{toolName}:Description";

    public static string InboxCategoryDisplayName(string categorySlug) => $"InboxCategory:{categorySlug}:DisplayName";

    public static string SeededMenuDisplayName(string menuKey) => $"SeededMenu:{menuKey}:DisplayName";

    public static string SeededMenuItemDisplayName(string menuKey, string itemSlug) => $"SeededMenu:{menuKey}:Item:{itemSlug}:DisplayName";

    public static string SeededCalendarDisplayName(string calendarKey) => $"SeededCalendar:{calendarKey}:DisplayName";
}
