using System;
using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Cache;

/// <summary>
/// Replaces one server's external tools in a catalog and leaves every other server alone.
/// </summary>
public static class McpCatalogServerSync
{
    public static List<MCPToolDescriptor> Apply(
        IEnumerable<MCPToolDescriptor>? existing,
        string serverKey,
        bool connectionSucceeded,
        IReadOnlyList<MCPToolDescriptor>? discovered)
    {
        if (!connectionSucceeded)
        {
            return (existing ?? Array.Empty<MCPToolDescriptor>()).ToList();
        }

        return Merge(existing, serverKey, discovered);
    }

    public static List<MCPToolDescriptor> Merge(
        IEnumerable<MCPToolDescriptor>? existing,
        string serverKey,
        IReadOnlyList<MCPToolDescriptor>? discovered)
    {
        var prefix = "external." + serverKey + ".";
        var kept = (existing ?? Array.Empty<MCPToolDescriptor>())
            .Where(tool => !tool.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (discovered is { Count: > 0 })
        {
            kept.AddRange(discovered);
        }

        return kept;
    }
}
