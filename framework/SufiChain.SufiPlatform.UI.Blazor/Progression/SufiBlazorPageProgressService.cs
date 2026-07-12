using System.Collections.Concurrent;
using SufiChain.SufiPlatform.UI.Blazor.Circuit;
using SufiChain.SufiPlatform.UI.Progression;
using Microsoft.Extensions.Logging;

namespace SufiChain.SufiPlatform.UI.Blazor.Progression;

/// <summary>
/// Implementation of IUiPageProgressService for managing page progress indicators.
/// </summary>
public class SufiBlazorPageProgressService : IUiPageProgressService
{
    private readonly ConcurrentDictionary<string, EventHandler<UiPageProgressEventArgs>> _handlersByCircuitId = new();
    private readonly IBlazorCircuitIdAccessor? _circuitIdAccessor;
    private readonly ILogger<SufiBlazorPageProgressService>? _logger;

    /// <summary>
    /// Event raised when progress changes (fallback for WebAssembly or when circuit ID is not available).
    /// </summary>
    public event EventHandler<UiPageProgressEventArgs>? ProgressChanged;

    public SufiBlazorPageProgressService(
        IBlazorCircuitIdAccessor? circuitIdAccessor = null,
        ILogger<SufiBlazorPageProgressService>? logger = null)
    {
        _circuitIdAccessor = circuitIdAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Registers a handler for a specific circuit.
    /// </summary>
    public void RegisterCircuitHandler(string circuitId, EventHandler<UiPageProgressEventArgs> handler)
    {
        _handlersByCircuitId[circuitId] = handler;
        _logger?.LogInformation("Registered PageProgress handler for circuit {CircuitId}", circuitId);
    }

    /// <summary>
    /// Unregisters the handler for a circuit.
    /// </summary>
    public void UnregisterCircuitHandler(string circuitId)
    {
        _handlersByCircuitId.TryRemove(circuitId, out _);
        _logger?.LogInformation("Unregistered PageProgress handler for circuit {CircuitId}", circuitId);
    }


    public Task SetProgressAsync(int? percentage, UiPageProgressType type = UiPageProgressType.Default)
    {
        var args = new UiPageProgressEventArgs(percentage, type, percentage.HasValue, false);
        InvokeHandlerForCurrentCircuit(args);
        return Task.CompletedTask;
    }

    public Task ShowIndeterminateAsync(UiPageProgressType type = UiPageProgressType.Default)
    {
        var args = new UiPageProgressEventArgs(null, type, true, true);
        InvokeHandlerForCurrentCircuit(args);
        return Task.CompletedTask;
    }

    public Task HideAsync()
    {
        var args = new UiPageProgressEventArgs(null, UiPageProgressType.Default, false, false);
        InvokeHandlerForCurrentCircuit(args);
        return Task.CompletedTask;
    }

    private void InvokeHandlerForCurrentCircuit(UiPageProgressEventArgs args)
    {
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (!string.IsNullOrEmpty(circuitId) && _handlersByCircuitId.TryGetValue(circuitId, out var handler))
        {
            handler(this, args);
        }
        else
        {
            // Fallback to global event (for WebAssembly or when circuit ID is not available)
            _logger?.LogWarning("PageProgress handler not found for circuit {CircuitId}, using fallback event", circuitId ?? "null");
            ProgressChanged?.Invoke(this, args);
        }
    }
}
