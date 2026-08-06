using System.Threading;
using SufiChain.SufiPlatform.UI.Blazor.Circuit;

namespace SufiChain.SufiPlatform.UI.Blazor.Server.Circuit;

/// <summary>
/// Blazor Server implementation that provides the current circuit ID via AsyncLocal.
/// Set by <see cref="SufiBlazorCircuitHandler"/> at the start of each inbound activity.
/// </summary>
public class BlazorServerCircuitIdAccessor : IBlazorCircuitIdAccessor
{
    private static readonly AsyncLocal<string?> CurrentCircuitIdStorage = new();

    /// <inheritdoc />
    public string? CurrentCircuitId => CurrentCircuitIdStorage.Value;

    /// <summary>
    /// Sets the current circuit ID for the async context. Called by the circuit handler.
    /// </summary>
    internal static void SetCurrentCircuitId(string? circuitId)
    {
        CurrentCircuitIdStorage.Value = circuitId;
    }
}
