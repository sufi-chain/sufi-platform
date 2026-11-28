namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// Source text the tools grid filters on. External tools include the server key
/// so a filter for the key matches as well as the display name.
/// </summary>
public static class McpToolSourceText
{
    public static string Format(string? source, string? toolName)
    {
        var display = source ?? string.Empty;
        if (!McpExternalToolName.TrySplit(toolName, out var serverKey, out _))
        {
            return display;
        }

        if (display.Contains(serverKey, StringComparison.OrdinalIgnoreCase))
        {
            return display;
        }

        return string.IsNullOrWhiteSpace(display) ? serverKey : $"{display} ({serverKey})";
    }
}
