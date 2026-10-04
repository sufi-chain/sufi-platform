using System.Globalization;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Stable host-default marker. The workspace name is not stable: production renamed
/// the seeded workspace away from <c>default</c>, and new tenants then inherited nothing.
/// </summary>
public static class HostDefaultWorkspaceMarker
{
    public const string IsDefaultProperty = "IsDefault";

    public static bool IsMarked(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!workspace.ExtraProperties.TryGetValue(IsDefaultProperty, out var value) || value == null)
        {
            return false;
        }

        if (value is bool flag)
        {
            return flag;
        }

        return bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var parsed) && parsed;
    }

    public static bool HasSeedVersion(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        return workspace.ExtraProperties.TryGetValue(DefaultWorkspaceManagedFieldReconciler.SeedVersionProperty, out var value)
               && value != null
               && !string.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture));
    }

    public static void Mark(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        workspace.SetProperty(IsDefaultProperty, true);
    }

    public static void Clear(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!workspace.ExtraProperties.ContainsKey(IsDefaultProperty))
        {
            return;
        }

        workspace.SetProperty(IsDefaultProperty, false);
    }

    public static bool MarkIfMissing(Workspace workspace)
    {
        if (IsMarked(workspace))
        {
            return false;
        }

        Mark(workspace);
        return true;
    }

    /// <summary>
    /// Prefer an explicit default, then the seeded workspace that survived a rename,
    /// then the configured name.
    /// </summary>
    public static Workspace? Select(IReadOnlyList<Workspace> workspaces, string? configuredName)
    {
        ArgumentNullException.ThrowIfNull(workspaces);

        var marked = workspaces.Where(IsMarked).OrderBy(workspace => workspace.Name, StringComparer.OrdinalIgnoreCase).ToList();
        if (marked.Count > 0)
        {
            return marked[0];
        }

        var seeded = workspaces.Where(HasSeedVersion).OrderBy(workspace => workspace.Name, StringComparer.OrdinalIgnoreCase).ToList();
        if (seeded.Count > 0)
        {
            return seeded[0];
        }

        if (string.IsNullOrWhiteSpace(configuredName))
        {
            return null;
        }

        var name = configuredName.Trim();
        return workspaces.FirstOrDefault(workspace =>
            string.Equals(workspace.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
