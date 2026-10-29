using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// Shared MCP JSON-RPC session for the remote transports. Subclasses only own how a
/// request reaches the server (one HTTP POST, or an SSE stream plus a message POST).
/// </summary>
public abstract class McpRemoteTransportClient : IMCPTransportClient
{
    private int _nextId;

    protected McpRemoteTransportClient(string endpoint, ILogger logger, TimeSpan? requestTimeout = null)
    {
        var timeout = requestTimeout.GetValueOrDefault(Timeout.InfiniteTimeSpan);
        RequestTimeout = timeout <= TimeSpan.Zero ? Timeout.InfiniteTimeSpan : timeout;
        Endpoint = Check.NotNullOrWhiteSpace(endpoint, nameof(endpoint));
        Logger = logger;

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw Connection($"MCP endpoint must be an absolute http or https URL. '{endpoint}' is not.");
        }

        EndpointUri = uri;
    }

    public abstract MCPTransportType TransportType { get; }

    public abstract bool IsConnected { get; }

    protected string Endpoint { get; }

    protected Uri EndpointUri { get; }

    protected ILogger Logger { get; }

    /// <summary>
    /// Deadline from <see cref="AITransportOptions"/>. <see cref="Timeout.InfiniteTimeSpan"/> means the
    /// call ends only when the caller's cancellation token is cancelled.
    /// </summary>
    protected TimeSpan RequestTimeout { get; }

    protected string NegotiatedProtocolVersion { get; private set; } = McpJsonRpc.RequestedProtocolVersion;

    protected string? SessionId { get; set; }

    public abstract Task ConnectAsync(CancellationToken cancellationToken = default);

    public abstract Task DisconnectAsync(CancellationToken cancellationToken = default);

    public async Task<List<MCPServerToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default,
        string? bearerToken = null)
    {
        var tools = new List<MCPServerToolDefinition>();
        string? cursor = null;
        for (var page = 0; page < 20; page++)
        {
            object? parameters = string.IsNullOrEmpty(cursor)
                ? null
                : new Dictionary<string, object?> { ["cursor"] = cursor };

            var result = await RequestAsync("tools/list", parameters, cancellationToken, bearerToken);
            if (result.ValueKind == JsonValueKind.Object
                && result.TryGetProperty("tools", out var listed))
            {
                tools.AddRange(McpJsonRpc.ReadTools(listed));
            }

            if (result.ValueKind != JsonValueKind.Object
                || !result.TryGetProperty("nextCursor", out var next)
                || next.ValueKind != JsonValueKind.String)
            {
                break;
            }

            cursor = next.GetString();
            if (string.IsNullOrEmpty(cursor))
            {
                break;
            }
        }

        return tools;
    }

    public async Task<MCPServerToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default,
        string? bearerToken = null)
    {
        try
        {
            var result = await RequestAsync(
                "tools/call",
                new { name = toolName, arguments = parameters },
                cancellationToken,
                bearerToken);

            var isError = result.ValueKind == JsonValueKind.Object
                && result.TryGetProperty("isError", out var flag)
                && flag.ValueKind == JsonValueKind.True;

            return new MCPServerToolResult
            {
                Success = !isError,
                Result = JsonSerializer.Deserialize<object>(result.GetRawText()),
                ErrorMessage = isError ? ReadToolError(result) : null
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new MCPServerToolResult
            {
                Success = false,
                ErrorMessage = McpSanitizedError.FromException(ex, _nextId > 0 ? _nextId.ToString() : null).ToClientMessage()
            };
        }
    }

    public abstract void Dispose();

    protected async Task CompleteInitializeAsync(CancellationToken cancellationToken)
    {
        var result = await RequestAsync(
            "initialize",
            new
            {
                protocolVersion = McpJsonRpc.RequestedProtocolVersion,
                capabilities = new { },
                clientInfo = new { name = "SufiAI", version = "1.0" }
            },
            cancellationToken,
            bearerToken: null);

        if (result.ValueKind == JsonValueKind.Object
            && result.TryGetProperty("protocolVersion", out var version)
            && version.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(version.GetString()))
        {
            NegotiatedProtocolVersion = version.GetString()!;
        }

        await NotifyAsync("notifications/initialized", cancellationToken, bearerToken: null);
    }

    protected void ApplyProtocolHeaders(HttpRequestMessage request, string? bearerToken)
    {
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Headers.TryAddWithoutValidation(McpJsonRpc.ProtocolHeader, NegotiatedProtocolVersion);
        if (!string.IsNullOrWhiteSpace(SessionId))
        {
            request.Headers.TryAddWithoutValidation(McpJsonRpc.SessionHeader, SessionId);
        }

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }

    protected BusinessException Connection(string message, Exception? inner = null)
    {
        var text = McpSanitizedError.Sanitize(message);
        return new BusinessException(AIErrorCodes.MCPServerConnectionFailed, text, innerException: inner)
            .WithData("Endpoint", Endpoint)
            .WithData("Error", text);
    }

    protected abstract Task<JsonElement> SendRequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken,
        string? bearerToken);

    protected abstract Task SendNotificationAsync(
        string method,
        CancellationToken cancellationToken,
        string? bearerToken);

    private Task<JsonElement> RequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken,
        string? bearerToken)
    {
        return WithRequestDeadlineAsync(
            cancellationToken,
            token => SendRequestAsync(method, parameters, token, bearerToken),
            $"Timed out waiting for MCP {method} from {Endpoint}.");
    }

    private Task NotifyAsync(string method, CancellationToken cancellationToken, string? bearerToken)
    {
        return WithRequestDeadlineAsync(
            cancellationToken,
            token => SendNotificationAsync(method, token, bearerToken),
            $"Timed out waiting for MCP {method} from {Endpoint}.");
    }

    /// <summary>
    /// Applies <see cref="RequestTimeout"/> when the host configured one. Otherwise the caller token is the only deadline.
    /// </summary>
    protected async Task<T> WithRequestDeadlineAsync<T>(
        CancellationToken cancellationToken,
        Func<CancellationToken, Task<T>> action,
        string timeoutMessage)
    {
        if (RequestTimeout == Timeout.InfiniteTimeSpan)
        {
            return await action(cancellationToken);
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(RequestTimeout);
        try
        {
            return await action(linked.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw Connection(timeoutMessage);
        }
    }

    protected Task WithRequestDeadlineAsync(
        CancellationToken cancellationToken,
        Func<CancellationToken, Task> action,
        string timeoutMessage)
    {
        return WithRequestDeadlineAsync(
            cancellationToken,
            async token =>
            {
                await action(token);
                return true;
            },
            timeoutMessage);
    }

    private int NextId() => Interlocked.Increment(ref _nextId);

    protected int TakeId() => NextId();

    private static string ReadToolError(JsonElement result)
    {
        if (result.ValueKind != JsonValueKind.Object
            || !result.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.Array)
        {
            return "The MCP tool returned an error.";
        }

        var parts = new List<string>();
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object
                && item.TryGetProperty("text", out var text)
                && text.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(text.GetString()))
            {
                parts.Add(text.GetString()!);
            }
        }

        var message = parts.Count == 0 ? "The MCP tool returned an error." : string.Join(' ', parts);
        return McpSanitizedError.Sanitize(message);
    }
}
