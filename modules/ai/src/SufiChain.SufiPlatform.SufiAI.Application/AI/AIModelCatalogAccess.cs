using SufiChain.SufiPlatform.SufiAI.Permissions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Workspace-only model catalog listing is called from workspace chat and workspace
/// readiness, which are granted <see cref="AIPermissions.WorkspaceChat.Default"/> or
/// <see cref="AIPermissions.Workspaces.Default"/> and not necessarily <see cref="AIPermissions.AI.Chat"/>.
/// Hooshvare-scoped listing stays authenticated and does not use this check.
/// </summary>
public static class AIModelCatalogAccess
{
    public static readonly string[] WorkspaceRoutePermissions =
    [
        AIPermissions.AI.Chat,
        AIPermissions.WorkspaceChat.Default,
        AIPermissions.Workspaces.Default
    ];

    public static bool AllowsWorkspaceRoutes(IEnumerable<string> grantedPermissions)
    {
        var granted = grantedPermissions as ISet<string> ?? new HashSet<string>(grantedPermissions);
        return WorkspaceRoutePermissions.Any(granted.Contains);
    }

    public static string DescribeDenial(bool isAuthenticated)
    {
        var required = string.Join(", ", WorkspaceRoutePermissions);
        return isAuthenticated
            ? $"Loading the model catalog requires one of these permissions: {required}."
            : "Loading the model catalog requires a signed-in user. The current principal is anonymous, so permission grants on the account are not applied. "
              + $"Required permissions: {required}.";
    }
}
