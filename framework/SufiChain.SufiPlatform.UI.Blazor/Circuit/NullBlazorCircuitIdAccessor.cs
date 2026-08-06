namespace SufiChain.SufiPlatform.UI.Blazor.Circuit;

/// <summary>
/// No-op implementation used when not in Blazor Server mode (e.g. WebAssembly).
/// Returns null for <see cref="CurrentCircuitId"/>, so the notification service
/// falls back to the global event.
/// </summary>
public class NullBlazorCircuitIdAccessor : IBlazorCircuitIdAccessor
{
    /// <inheritdoc />
    public string? CurrentCircuitId => null;
}
