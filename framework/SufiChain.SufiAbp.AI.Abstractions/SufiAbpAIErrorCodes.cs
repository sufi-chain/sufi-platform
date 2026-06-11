namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Error codes thrown by SufiAbp AI abstractions and their Null fallbacks.
/// </summary>
public static class SufiAbpAIErrorCodes
{
    /// <summary>
    /// Error code namespace prefix.
    /// </summary>
    public const string Prefix = "SufiAbpAI";

    /// <summary>
    /// Thrown when an AI operation is invoked but no AI provider implementation
    /// (e.g. the AIManagement module) is installed in the host.
    /// </summary>
    public const string ProviderNotAvailable = Prefix + ":ProviderNotAvailable";

    /// <summary>
    /// Thrown when the requested workspace cannot be resolved or is inactive.
    /// </summary>
    public const string WorkspaceNotAvailable = Prefix + ":WorkspaceNotAvailable";

    /// <summary>
    /// Thrown when the requested tool is unknown for the workspace.
    /// </summary>
    public const string ToolNotFound = Prefix + ":ToolNotFound";
}
