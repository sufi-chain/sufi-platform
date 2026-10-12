namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Compares the decisions model id that was sent to the server with the one that
/// came back on the saved workspace, so the editor can tell whether the Decisions
/// route was actually kept.
/// </summary>
public static class WorkspaceDecisionsCheck
{
    public static bool IsKept(string? sent, string? saved)
    {
        var sentBlank = string.IsNullOrWhiteSpace(sent);
        var savedBlank = string.IsNullOrWhiteSpace(saved);

        if (sentBlank && savedBlank)
        {
            return true;
        }

        if (sentBlank || savedBlank)
        {
            return false;
        }

        return string.Equals(sent!.Trim(), saved!.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
