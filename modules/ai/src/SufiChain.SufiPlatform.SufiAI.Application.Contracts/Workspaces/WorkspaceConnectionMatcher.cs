namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Decides whether a model route effectively uses its workspace connection.
/// Own connection means the route has its own API key. A copied or stale endpoint
/// without a key still uses the workspace credential, so the list and the editor agree.
/// </summary>
public static class WorkspaceConnectionMatcher
{
    public static bool UsesWorkspaceConnection(string? routeEndpoint, bool routeHasApiKey, string? workspaceEndpoint)
    {
        _ = routeEndpoint;
        _ = workspaceEndpoint;
        return !routeHasApiKey;
    }
}
