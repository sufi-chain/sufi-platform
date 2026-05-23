namespace SufiChain.SufiAbp.CLI.Args;

/// <summary>
/// Solution structure kind for generated projects.
/// </summary>
public enum SolutionKind
{
    /// <summary>
    /// Minimal 3-project structure: App (UI + API + Auth + DB), App.Client, Application.Contracts.
    /// </summary>
    Single,

    /// <summary>
    /// Full DDD project structure with separate Domain, Application, HttpApi, and infrastructure layers.
    /// Can be tiered (separate hosts) or non-tiered (single host with direct DB access).
    /// </summary>
    Layered
}
