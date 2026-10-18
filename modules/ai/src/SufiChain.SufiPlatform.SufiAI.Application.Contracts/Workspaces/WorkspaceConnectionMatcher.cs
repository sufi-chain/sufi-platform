using System;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Decides whether a model route effectively uses its workspace connection.
/// A route without its own key whose endpoint is empty or equal to the workspace endpoint
/// (seeded routes copy the workspace URL) is treated as inherited.
/// </summary>
public static class WorkspaceConnectionMatcher
{
    public static bool UsesWorkspaceConnection(string? routeEndpoint, bool routeHasApiKey, string? workspaceEndpoint)
    {
        if (routeHasApiKey)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(routeEndpoint))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(workspaceEndpoint)
               && string.Equals(Normalize(routeEndpoint), Normalize(workspaceEndpoint), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string value) => value.Trim().TrimEnd('/');
}
