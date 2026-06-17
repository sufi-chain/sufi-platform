namespace SufiChain.SufiAbp.CLI.Args;

/// <summary>
/// Solution structure kind for generated projects.
/// </summary>
public enum SolutionKind
{
    /// <summary>
    /// DDD solution hosted by Blazor.WebApp without separate HttpApi.Host/AuthServer hosts.
    /// </summary>
    WebApp,

    /// <summary>
    /// Full DDD project structure with separate Domain, Application, HttpApi, and infrastructure layers.
    /// Can be tiered (separate hosts) or non-tiered (WebApp + HttpApi.Host).
    /// </summary>
    Layered
}
