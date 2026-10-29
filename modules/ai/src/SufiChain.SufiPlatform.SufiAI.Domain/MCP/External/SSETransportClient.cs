using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// Legacy MCP HTTP+SSE transport (protocol revision 2024-11-05).
/// The client GETs the SSE endpoint, reads the <c>endpoint</c> event, and POSTs JSON-RPC
/// messages to that URI. Results arrive as <c>message</c> events on the SSE stream.
/// This client does not call <c>/health</c>, <c>/tools</c>, or <c>/tools/call</c>.
/// </summary>
public sealed class SSETransportClient : McpRemoteTransportClient
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _pending = new();
    private CancellationTokenSource? _lifetime;
    private HttpResponseMessage? _response;
    private Task? _readTask;
    private TaskCompletionSource<Uri>? _endpointSource;
    private Uri? _postEndpoint;
    private bool _initialized;

    public SSETransportClient(
        string endpoint,
        HttpClient httpClient,
        ILogger<SSETransportClient> logger,
        TimeSpan? requestTimeout = null)
        : base(endpoint, logger, requestTimeout)
    {
        _httpClient = httpClient;
        // The GET stays open for the life of the session. Per-request cancellation is separate.
        _httpClient.Timeout = Timeout.InfiniteTimeSpan;
    }

    public override MCPTransportType TransportType => MCPTransportType.SSE;

    public override bool IsConnected => _initialized && _readTask is { IsCompleted: false };

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await OpenStreamAsync(cancellationToken);
            await CompleteInitializeAsync(cancellationToken);
            _initialized = true;
            Logger.LogInformation("Connected to MCP server via SSE: {Endpoint}", Endpoint);
        }
        catch (Exception ex) when (ex is not BusinessException and not OperationCanceledException)
        {
            await DisconnectCoreAsync(cancellationToken);
            Logger.LogError(ex, "Failed to connect to MCP server: {Endpoint}", Endpoint);
            throw Connection(ex.Message, ex);
        }
        catch
        {
            await DisconnectCoreAsync(cancellationToken);
            throw;
        }
    }

    public override Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return DisconnectCoreAsync(cancellationToken);
    }

    public override void Dispose()
    {
        try
        {
            _lifetime?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already torn down.
        }

        _response?.Dispose();
        _response = null;
        _initialized = false;
        FailPending(Connection($"MCP SSE connection to {Endpoint} was closed."));
    }

    protected override async Task<JsonElement> SendRequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken,
        string? bearerToken)
    {
        if (_postEndpoint is null)
        {
            throw Connection("MCP SSE connection is not open.");
        }

        var id = TakeId();
        var waiter = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = waiter;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _postEndpoint)
            {
                Content = new StringContent(
                    McpJsonRpc.CreateRequest(id, method, parameters),
                    Encoding.UTF8,
                    "application/json")
            };
            ApplyProtocolHeaders(request, bearerToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var failure = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
                throw Connection(McpJsonRpc.FormatHttpFailure(response, failure));
            }

            var body = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
            if (McpJsonRpc.TryParseMessage(body, out var inline) && inline.Id == id)
            {
                if (inline.Error is not null)
                {
                    throw Connection(inline.Error);
                }

                if (inline.HasResult)
                {
                    return inline.Result;
                }
            }

            return await waiter.Task.WaitAsync(cancellationToken);
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    protected override async Task SendNotificationAsync(
        string method,
        CancellationToken cancellationToken,
        string? bearerToken)
    {
        if (_postEndpoint is null)
        {
            throw Connection("MCP SSE connection is not open.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _postEndpoint)
        {
            Content = new StringContent(McpJsonRpc.CreateNotification(method), Encoding.UTF8, "application/json")
        };
        ApplyProtocolHeaders(request, bearerToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted or HttpStatusCode.NoContent)
        {
            return;
        }

        var body = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
        throw Connection(McpJsonRpc.FormatHttpFailure(response, body));
    }

    private async Task OpenStreamAsync(CancellationToken cancellationToken)
    {
        _lifetime = new CancellationTokenSource();
        _endpointSource = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var request = new HttpRequestMessage(HttpMethod.Get, EndpointUri);
        request.Headers.Accept.ParseAdd("text/event-stream");

        var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var failure = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
            response.Dispose();
            throw Connection(McpJsonRpc.FormatHttpFailure(response, failure));
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            var body = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
            response.Dispose();
            throw Connection(
                $"MCP SSE endpoint returned '{mediaType ?? "no content type"}' instead of text/event-stream. {McpJsonRpc.Snippet(body)}");
        }

        _response = response;
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        _readTask = ReadLoopAsync(stream, _lifetime.Token);

        _postEndpoint = await WithRequestDeadlineAsync(
            cancellationToken,
            token => _endpointSource.Task.WaitAsync(token),
            $"Timed out waiting for the MCP SSE endpoint event from {Endpoint}.");
    }

    private async Task ReadLoopAsync(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var frame in SseEventReader.ReadAsync(stream, cancellationToken))
            {
                if (IsEndpointFrame(frame))
                {
                    if (!TryResolveEndpoint(frame.Data, out var uri, out var error))
                    {
                        _endpointSource?.TrySetException(Connection(error!));
                        return;
                    }

                    _endpointSource?.TrySetResult(uri!);
                    continue;
                }

                if (!McpJsonRpc.TryParseMessage(frame.Data, out var message) || message.Id is not int id)
                {
                    continue;
                }

                if (!_pending.TryGetValue(id, out var waiter))
                {
                    continue;
                }

                if (message.Error is not null)
                {
                    waiter.TrySetException(Connection(message.Error));
                }
                else if (message.HasResult)
                {
                    waiter.TrySetResult(message.Result);
                }
            }

            _endpointSource?.TrySetException(Connection($"MCP server closed the SSE stream at {Endpoint} before sending an endpoint event."));
            FailPending(Connection($"MCP server closed the SSE stream at {Endpoint}."));
        }
        catch (OperationCanceledException)
        {
            FailPending(Connection($"MCP SSE connection to {Endpoint} was closed."));
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            var connection = Connection(ex.Message, ex);
            _endpointSource?.TrySetException(connection);
            FailPending(connection);
        }
    }

    private bool IsEndpointFrame(SseEvent frame)
    {
        if (string.Equals(frame.EventName, "endpoint", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (_endpointSource is null || _endpointSource.Task.IsCompleted)
        {
            return false;
        }

        var data = frame.Data.Trim();
        return data.StartsWith('/')
            || data.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || data.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryResolveEndpoint(string data, out Uri? uri, out string? error)
    {
        uri = null;
        error = null;
        var text = data.Trim();
        if (!Uri.TryCreate(EndpointUri, text, out var resolved)
            || !resolved.IsAbsoluteUri
            || (resolved.Scheme != Uri.UriSchemeHttp && resolved.Scheme != Uri.UriSchemeHttps))
        {
            error = $"MCP server sent an SSE endpoint that is not an http(s) URL: '{text}'.";
            return false;
        }

        if (!SameOrigin(EndpointUri, resolved))
        {
            error = "MCP server sent an SSE endpoint on a different origin than "
                + $"{EndpointUri.GetLeftPart(UriPartial.Authority)}: '{resolved}'.";
            return false;
        }

        uri = resolved;
        return true;
    }

    private static bool SameOrigin(Uri configured, Uri resolved) =>
        string.Equals(configured.Scheme, resolved.Scheme, StringComparison.OrdinalIgnoreCase)
        && string.Equals(configured.Host, resolved.Host, StringComparison.OrdinalIgnoreCase)
        && configured.Port == resolved.Port;

    private async Task DisconnectCoreAsync(CancellationToken cancellationToken = default)
    {
        _initialized = false;
        _postEndpoint = null;

        try
        {
            _lifetime?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already torn down.
        }

        _response?.Dispose();
        _response = null;

        if (_readTask != null)
        {
            try
            {
                await _readTask.WaitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "MCP SSE read loop ended while disconnecting from {Endpoint}", Endpoint);
            }

            _readTask = null;
        }

        _lifetime?.Dispose();
        _lifetime = null;
        FailPending(Connection($"MCP SSE connection to {Endpoint} was closed."));
    }

    private void FailPending(Exception exception)
    {
        foreach (var pending in _pending)
        {
            pending.Value.TrySetException(exception);
        }

        _pending.Clear();
    }
}
