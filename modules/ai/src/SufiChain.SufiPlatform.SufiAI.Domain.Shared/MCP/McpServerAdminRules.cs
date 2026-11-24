using System;
using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// MCP admin checks. The stable key is already unique per tenant; the display name is unique too,
/// compared without case, so two servers cannot both be called the same thing.
/// There is no unique index: existing duplicate names stay until the next save.
/// </summary>
public static class McpServerAdminRules
{
    public static bool RequiresEndpoint(string? transportType) =>
        !string.IsNullOrWhiteSpace(transportType)
        && !string.Equals(transportType, "STDIO", StringComparison.OrdinalIgnoreCase);

    public static bool IsAbsoluteHttpUrl(string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return false;
        }

        return Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsDuplicateName(
        IEnumerable<(Guid Id, string Name)> existing,
        Guid? currentId,
        string name)
    {
        var trimmed = name.Trim();
        return existing.Any(server =>
            server.Id != currentId
            && string.Equals(server.Name.Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
