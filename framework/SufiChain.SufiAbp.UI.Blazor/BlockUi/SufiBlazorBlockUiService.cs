using System.Collections.Concurrent;
using SufiChain.SufiAbp.UI.Blazor.Circuit;
using SufiChain.SufiAbp.UI.BlockUi;
using Microsoft.Extensions.Logging;

namespace SufiChain.SufiAbp.UI.Blazor.BlockUi;

/// <summary>
/// Implementation of IBlockUiService using event-based communication with SufiAbpOverlayHost.
/// </summary>
public class SufiBlazorBlockUiService : IBlockUiService
{
    private readonly ConcurrentDictionary<string, Action<BlockUiState>> _handlersByCircuitId = new();
    private readonly IBlazorCircuitIdAccessor? _circuitIdAccessor;
    private readonly ILogger<SufiBlazorBlockUiService>? _logger;

    /// <summary>
    /// Event raised when the block state changes (fallback for WebAssembly or when circuit ID is not available).
    /// </summary>
    public event Action<BlockUiState>? OnBlockStateChanged;

    public SufiBlazorBlockUiService(
        IBlazorCircuitIdAccessor? circuitIdAccessor = null,
        ILogger<SufiBlazorBlockUiService>? logger = null)
    {
        _circuitIdAccessor = circuitIdAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Registers a handler for a specific circuit.
    /// </summary>
    public void RegisterCircuitHandler(string circuitId, Action<BlockUiState> handler)
    {
        _handlersByCircuitId[circuitId] = handler;
        _logger?.LogInformation("Registered BlockUI handler for circuit {CircuitId}", circuitId);
    }

    /// <summary>
    /// Unregisters the handler for a circuit.
    /// </summary>
    public void UnregisterCircuitHandler(string circuitId)
    {
        _handlersByCircuitId.TryRemove(circuitId, out _);
        _logger?.LogInformation("Unregistered BlockUI handler for circuit {CircuitId}", circuitId);
    }

    public Task BlockAsync(string? selectors = null, bool busy = false)
    {
        var state = new BlockUiState(true, selectors, busy);
        InvokeHandlerForCurrentCircuit(state);
        return Task.CompletedTask;
    }

    public Task UnblockAsync()
    {
        var state = new BlockUiState(false, null, false);
        InvokeHandlerForCurrentCircuit(state);
        return Task.CompletedTask;
    }

    private void InvokeHandlerForCurrentCircuit(BlockUiState state)
    {
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (!string.IsNullOrEmpty(circuitId) && _handlersByCircuitId.TryGetValue(circuitId, out var handler))
        {
            handler(state);
        }
        else
        {
            // Fallback to global event (for WebAssembly or when circuit ID is not available)
            _logger?.LogWarning("BlockUI handler not found for circuit {CircuitId}, using fallback event", circuitId ?? "null");
            OnBlockStateChanged?.Invoke(state);
        }
    }
}

/// <summary>
/// Represents the block UI state.
/// </summary>
public record BlockUiState(bool IsBlocked, string? Selectors, bool ShowBusy);
