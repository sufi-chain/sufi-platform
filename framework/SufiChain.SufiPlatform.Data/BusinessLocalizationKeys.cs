namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Canonical key patterns for business-tier localization stored in <c>LocalizationText</c>.
/// </summary>
public static class BusinessLocalizationKeys
{
    public static string FileStructureDisplayName(string structureKey) => $"Structure:{structureKey}:DisplayName";

    public static string FileStructureDescription(string structureKey) => $"Structure:{structureKey}:Description";

    public static string HooshvareDisplayName(string hooshvareKey) => $"Hooshvare:{hooshvareKey}:DisplayName";

    public static string HooshvareDescription(string hooshvareKey) => $"Hooshvare:{hooshvareKey}:Description";

    public static string HooshvareSystemPrompt(string hooshvareKey) => $"Hooshvare:{hooshvareKey}:SystemPrompt";

    public static string HooshvareShortcut(string hooshvareKey, string shortcutId) => $"Hooshvare:{hooshvareKey}:Shortcut:{shortcutId}";

    [Obsolete("Use hooshvare Key overloads. Guid-based keys break tenant-scoped hooshvares.")]
    public static string HooshvareDisplayName(Guid hooshvareId) => $"Hooshvare:{hooshvareId:D}:DisplayName";

    [Obsolete("Use hooshvare Key overloads.")]
    public static string HooshvareDescription(Guid hooshvareId) => $"Hooshvare:{hooshvareId:D}:Description";

    [Obsolete("Use hooshvare Key overloads.")]
    public static string HooshvareSystemPrompt(Guid hooshvareId) => $"Hooshvare:{hooshvareId:D}:SystemPrompt";

    [Obsolete("Use hooshvare Key overloads.")]
    public static string HooshvareShortcut(Guid hooshvareId, string shortcutId) => $"Hooshvare:{hooshvareId:D}:Shortcut:{shortcutId}";

    public static string McpToolDisplayName(string toolName) => $"MCPTool:{toolName}:DisplayName";

    public static string McpToolDescription(string toolName) => $"MCPTool:{toolName}:Description";

    public static string InboxCategoryDisplayName(string categorySlug) => $"InboxCategory:{categorySlug}:DisplayName";

    public static string SeededMenuDisplayName(string menuKey) => $"SeededMenu:{menuKey}:DisplayName";

    public static string SeededMenuItemDisplayName(string menuKey, string itemSlug) => $"SeededMenu:{menuKey}:Item:{itemSlug}:DisplayName";

    public static string SeededCalendarDisplayName(string calendarKey) => $"SeededCalendar:{calendarKey}:DisplayName";
}
