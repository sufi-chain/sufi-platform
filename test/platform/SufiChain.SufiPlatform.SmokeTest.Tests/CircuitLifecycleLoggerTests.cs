using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Server.Circuit;
using Xunit;

namespace SufiChain.SufiPlatform.SmokeTest;

public class CircuitLifecycleLoggerTests
{
    [Fact]
    public async Task Logs_open_and_close_and_a_second_close_does_not_throw()
    {
        var logger = new ListLogger();
        var handler = new CircuitLifecycleLogger(logger);
        var circuit = CreateCircuit("circuit-1");

        await handler.OnCircuitOpenedAsync(circuit, CancellationToken.None);
        await handler.OnConnectionUpAsync(circuit, CancellationToken.None);
        await handler.OnConnectionDownAsync(circuit, CancellationToken.None);
        await handler.OnCircuitClosedAsync(circuit, CancellationToken.None);
        await handler.OnCircuitClosedAsync(circuit, CancellationToken.None);

        logger.Messages.Count.ShouldBe(5);
        logger.Messages[0].ShouldContain("opened");
        logger.Messages[0].ShouldContain("circuit-1");
        logger.Messages[3].ShouldContain("closed");
        logger.Messages[4].ShouldContain("closed");
        string.Join('\n', logger.Messages).ShouldNotContain("admin");
    }

    private static Circuit CreateCircuit(string circuitId)
    {
        var assembly = typeof(Circuit).Assembly;
        var circuitIdType = assembly.GetType("Microsoft.AspNetCore.Components.Server.Circuits.CircuitId");
        circuitIdType.ShouldNotBeNull();
        var id = Activator.CreateInstance(
            circuitIdType!,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: ["secret", circuitId],
            culture: null);

        var hostType = assembly.GetType("Microsoft.AspNetCore.Components.Server.Circuits.CircuitHost");
        hostType.ShouldNotBeNull();
        var host = RuntimeHelpers.GetUninitializedObject(hostType!);
        hostType!.GetField("<CircuitId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(host, id);

        var circuit = (Circuit)RuntimeHelpers.GetUninitializedObject(typeof(Circuit));
        typeof(Circuit).GetField("_circuitHost", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(circuit, host);
        return circuit;
    }

    private sealed class ListLogger : ILogger<CircuitLifecycleLogger>
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
