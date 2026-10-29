using System.Runtime.CompilerServices;
using System.Text;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

internal readonly record struct SseEvent(string? EventName, string Data);

/// <summary>
/// Reads Server-Sent Events frames (<c>event</c>, <c>data</c>, blank line) from an MCP response.
/// Comment lines that start with ':' are ignored.
/// </summary>
internal static class SseEventReader
{
    public static async IAsyncEnumerable<SseEvent> ReadAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        string? eventName = null;
        var data = new StringBuilder();
        var reading = false;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                if (reading)
                {
                    yield return new SseEvent(eventName, data.ToString());
                }

                yield break;
            }

            if (line.Length == 0)
            {
                if (!reading)
                {
                    continue;
                }

                yield return new SseEvent(eventName, data.ToString());
                eventName = null;
                data.Clear();
                reading = false;
                continue;
            }

            if (line[0] == ':')
            {
                continue;
            }

            var separator = line.IndexOf(':');
            var field = separator < 0 ? line : line[..separator];
            var value = separator < 0 ? string.Empty : line[(separator + 1)..];
            if (value.StartsWith(' '))
            {
                value = value[1..];
            }

            if (field.Equals("event", StringComparison.Ordinal))
            {
                eventName = value;
                reading = true;
            }
            else if (field.Equals("data", StringComparison.Ordinal))
            {
                if (data.Length > 0)
                {
                    data.Append('\n');
                }

                data.Append(value);
                reading = true;
            }
        }
    }
}
