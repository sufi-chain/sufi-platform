namespace SufiChain.SufiAbp.CLI.Args;

/// <summary>
/// Types of hosts that can be included in a generated solution.
/// </summary>
public enum HostType
{
    /// <summary>
    /// Backend API host (HttpApi.Host) - API-only, JWT validation pointing to AuthServer.
    /// Always included for tiered architecture.
    /// </summary>
    HttpApi,
    
    /// <summary>
    /// Admin panel (Blazor.WebApp) with SufiTheme - default UI host.
    /// In tiered: OIDC client pointing to AuthServer.
    /// In non-tiered: UI + API + Auth + DB all-in-one.
    /// </summary>
    WebApp,
    
    /// <summary>
    /// Dedicated Blazor Server identity host (AuthServer) - handles OIDC authority,
    /// Login/Register pages, OpenIddict server. Only used in tiered architecture.
    /// </summary>
    AuthServer,
    
    /// <summary>
    /// Public website (Blazor.WebSite) with dynamic layouts.
    /// In tiered: OIDC client pointing to AuthServer.
    /// </summary>
    WebSite,
    
    /// <summary>
    /// MVC/Razor Pages host (Web) - traditional server-rendered UI.
    /// </summary>
    Web
}
