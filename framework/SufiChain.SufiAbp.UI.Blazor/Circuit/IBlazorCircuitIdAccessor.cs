namespace SufiChain.SufiAbp.UI.Blazor.Circuit;

/// <summary>
/// Provides access to the current Blazor Server circuit ID when running in Server mode.
/// When not available (e.g. WebAssembly), <see cref="CurrentCircuitId"/> returns null.
/// Used to isolate overlay notifications (toasts, block UI, etc.) per circuit so one
/// user's actions do not show overlays in another user's browser.
/// </summary>
public interface IBlazorCircuitIdAccessor
{
    /// <summary>
    /// Gets the current circuit ID, or null when not in Blazor Server or when the
    /// circuit context is not yet established.
    /// </summary>
    string? CurrentCircuitId { get; }
}
