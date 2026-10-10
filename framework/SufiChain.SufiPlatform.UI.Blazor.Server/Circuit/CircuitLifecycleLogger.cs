using System.Diagnostics;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using BlazorCircuit = Microsoft.AspNetCore.Components.Server.Circuits.Circuit;

namespace SufiChain.SufiPlatform.UI.Blazor.Server.Circuit;

/// <summary>
/// Logs circuit open, transport up/down, and close with the circuit id and lifetime.
/// Does not record user, tenant, or connection-token values.
/// Leaves <see cref="BlazorServerPrincipalCircuitHandler.Order"/> at <see cref="int.MinValue"/>
/// so the principal is still applied before this handler.
/// </summary>
public class CircuitLifecycleLogger : CircuitHandler
{
    private readonly ILogger<CircuitLifecycleLogger> _logger;
    private long _openedTimestamp;

    public CircuitLifecycleLogger(ILogger<CircuitLifecycleLogger> logger)
    {
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(BlazorCircuit circuit, CancellationToken cancellationToken)
    {
        _openedTimestamp = Stopwatch.GetTimestamp();
        Log("opened", circuit);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(BlazorCircuit circuit, CancellationToken cancellationToken)
    {
        Log("connection up", circuit);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(BlazorCircuit circuit, CancellationToken cancellationToken)
    {
        Log("connection down", circuit);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(BlazorCircuit circuit, CancellationToken cancellationToken)
    {
        // A second close from circuit teardown must not throw.
        Log("closed", circuit);
        return Task.CompletedTask;
    }

    private void Log(string change, BlazorCircuit? circuit)
    {
        var lifetimeMs = _openedTimestamp == 0
            ? 0
            : (long)Stopwatch.GetElapsedTime(_openedTimestamp).TotalMilliseconds;
        _logger.LogInformation(
            "Blazor circuit {Change}. CircuitId={CircuitId} LifetimeMs={LifetimeMs}",
            change,
            circuit?.Id,
            lifetimeMs);
    }
}
