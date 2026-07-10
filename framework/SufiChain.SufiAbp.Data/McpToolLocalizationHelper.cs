using Microsoft.Extensions.Localization;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Resolves MCP tool display names and descriptions from business-tier localization keys.
/// </summary>
public static class McpToolLocalizationHelper
{
    public static string ResolveToolDisplayName(
        IStringLocalizerFactory stringLocalizerFactory,
        string toolName,
        string? displayNameKey = null)
    {
        var key = displayNameKey ?? BusinessLocalizationKeys.McpToolDisplayName(toolName);
        return BusinessLocalizationHelper.ResolveText(
            stringLocalizerFactory,
            McpToolLocalizationResourceMap.GetResourceName(toolName),
            key,
            toolName);
    }

    public static string ResolveToolDescription(
        IStringLocalizerFactory stringLocalizerFactory,
        string toolName,
        string fallbackDescription)
    {
        return BusinessLocalizationHelper.ResolveText(
            stringLocalizerFactory,
            McpToolLocalizationResourceMap.GetResourceName(toolName),
            BusinessLocalizationKeys.McpToolDescription(toolName),
            fallbackDescription);
    }

    public static string ResolveToolSource(
        IStringLocalizerFactory stringLocalizerFactory,
        string toolName,
        string fallbackSource)
    {
        var resourceName = McpToolLocalizationResourceMap.GetResourceName(toolName);
        var moduleSourceKey = McpToolLocalizationResourceMap.GetModuleSourceLocalizationKey(toolName, fallbackSource);
        if (string.IsNullOrWhiteSpace(moduleSourceKey))
        {
            return fallbackSource;
        }

        return BusinessLocalizationHelper.ResolveText(
            stringLocalizerFactory,
            resourceName,
            moduleSourceKey,
            fallbackSource);
    }

    public static string ResolveToolType(
        IStringLocalizerFactory stringLocalizerFactory,
        string toolType)
    {
        if (string.IsNullOrWhiteSpace(toolType))
        {
            return string.Empty;
        }

        var key = $"MCPToolType:{toolType}";
        return BusinessLocalizationHelper.ResolveText(
            stringLocalizerFactory,
            McpToolLocalizationResourceMap.DefaultResourceName,
            key,
            toolType);
    }
}
