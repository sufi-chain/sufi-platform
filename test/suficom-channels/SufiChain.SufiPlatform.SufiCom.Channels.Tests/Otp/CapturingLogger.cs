using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Otp;

/// <summary>
/// Records formatted log lines and any attached exception text.
/// </summary>
internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<string> Lines { get; } = new();

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var line = formatter(state, exception);
        if (exception != null)
        {
            line = line + " " + exception;
        }

        Lines.Add(line);
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
