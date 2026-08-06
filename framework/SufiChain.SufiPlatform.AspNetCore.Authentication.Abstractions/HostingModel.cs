namespace SufiChain.SufiPlatform.AspNetCore.Authentication;

/// <summary>
/// Represents the hosting model for a Blazor component.
/// Used to determine the correct authentication flow.
/// </summary>
public enum HostingModel
{
    /// <summary>
    /// Component is running on the server (Blazor Server).
    /// </summary>
    Server,

    /// <summary>
    /// Component is running in the browser (Blazor WebAssembly).
    /// </summary>
    WebAssembly
}
