namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Maps MCP tool names to the owning module localization resource for business-tier labels.
/// </summary>
public static class McpToolLocalizationResourceMap
{
    public const string DefaultResourceName = "AI";

    private static readonly (string Prefix, string ResourceName)[] PrefixMappings =
    {
        ("calendar.", "Calendar"),
        ("contacts.", "SufiCRMContacts"),
        ("forms.", "SufiForms"),
        ("cms.", "SufiCMS"),
        ("helpdesk.", "KnowledgeBase"),
    };

    private static readonly (string Prefix, string ModuleSourceKey)[] ModuleSourceKeys =
    {
        ("calendar.", "MCPTool:Module:Calendar"),
        ("contacts.", "MCPTool:Module:Contacts"),
        ("forms.", "MCPTool:Module:Forms"),
        ("cms.", "MCPTool:Module:Cms"),
        ("helpdesk.", "MCPTool:Module:KnowledgeBase"),
    };

    public static string GetResourceName(string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return DefaultResourceName;
        }

        foreach (var (prefix, resourceName) in PrefixMappings)
        {
            if (toolName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return resourceName;
            }
        }

        return DefaultResourceName;
    }

    public static string? GetModuleSourceLocalizationKey(string toolName, string? fallbackSource = null)
    {
        if (!string.IsNullOrWhiteSpace(toolName))
        {
            foreach (var (prefix, moduleSourceKey) in ModuleSourceKeys)
            {
                if (toolName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return moduleSourceKey;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(fallbackSource))
        {
            return null;
        }

        if (fallbackSource.StartsWith("Calendar", StringComparison.OrdinalIgnoreCase))
        {
            return "MCPTool:Module:Calendar";
        }

        if (fallbackSource.StartsWith("Contacts", StringComparison.OrdinalIgnoreCase) ||
            fallbackSource.Contains("Contact", StringComparison.OrdinalIgnoreCase))
        {
            return "MCPTool:Module:Contacts";
        }

        if (fallbackSource.StartsWith("Forms", StringComparison.OrdinalIgnoreCase))
        {
            return "MCPTool:Module:Forms";
        }

        if (fallbackSource.StartsWith("CMS", StringComparison.OrdinalIgnoreCase) ||
            fallbackSource.Contains("SufiCMS", StringComparison.OrdinalIgnoreCase))
        {
            return "MCPTool:Module:Cms";
        }

        if (fallbackSource.Contains("Knowledge", StringComparison.OrdinalIgnoreCase) ||
            fallbackSource.StartsWith("HelpDesk", StringComparison.OrdinalIgnoreCase))
        {
            return "MCPTool:Module:KnowledgeBase";
        }

        return null;
    }
}
