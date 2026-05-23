using Microsoft.AspNetCore.Components.Server.Circuits;

namespace SufiChain.SufiAbp.UI.Blazor.Server.Circuit;

/// <summary>
/// Sets the current circuit ID at the start of each inbound activity so overlay
/// services (toasts, block UI, etc.) can isolate notifications per user/session.
/// </summary>
public class SufiAbpBlazorCircuitHandler : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            var circuitId = context.Circuit.Id;
            BlazorServerCircuitIdAccessor.SetCurrentCircuitId(circuitId);
            try
            {
                await next(context);
            }
            finally
            {
                BlazorServerCircuitIdAccessor.SetCurrentCircuitId(null);
            }
        };
    }
}
