using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// MCP Streamable HTTP transport (protocol revision 2025-03-26).
/// Every JSON-RPC message is an HTTP POST to the single MCP endpoint. The server answers
/// with either <c>application/json</c> or a <c>text/event-stream</c> that carries the response.
/// A <c>Mcp-Session-Id</c> returned by <c>initialize</c> is sent on every later request.
/// </summary>
public sealed class StreamableHttpTransportClient : McpRemoteTransportClient
{
    private readonly HttpClient _httpClient;
    private bool _connected;

    public StreamableHttpTransportClient(
        string endpoint,
        HttpClient httpClient,
        ILogger<StreamableHttpTransportClient> logger)
        : base(endpoint, logger)
    {
        _httpClient = httpClient;
    }

    public override MCPTransportType TransportType => MCPTransportType.HTTP;

    public override bool IsConnected => _connected;

    public override async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await CompleteInitializeAsync(cancellationToken);
            _connected = true;
            Logger.LogInformation("Connected to MCP server via streamable HTTP: {Endpoint}", Endpoint);
        }
        catch (Exception ex) when (ex is not BusinessException and not OperationCanceledException)
        {
            await DisconnectAsync(CancellationToken.None);
            Logger.LogError(ex, "Failed to connect to MCP server: {Endpoint}", Endpoint);
            throw Connection(ex.Message, ex);
        }
        catch
        {
            await DisconnectAsync(CancellationToken.None);
            throw;
        }
    }

    public override async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _connected = false;
        if (string.IsNullOrWhiteSpace(SessionId))
        {
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, EndpointUri);
            ApplyProtocolHeaders(request, bearerToken: null);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to close the MCP session at {Endpoint}", Endpoint);
        }
        finally
        {
            SessionId = null;
        }
    }

    public override void Dispose()
    {
        _connected = false;
    }

    protected override async Task<JsonElement> SendRequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken,
        string? bearerToken)
    {
        var id = TakeId();
        using var request = new HttpRequestMessage(HttpMethod.Post, EndpointUri)
        {
            Content = new StringContent(McpJsonRpc.CreateRequest(id, method, parameters), Encoding.UTF8, "application/json")
        };
        ApplyProtocolHeaders(request, bearerToken);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        RememberSession(response);
        return await ReadResultAsync(response, id, cancellationToken);
    }

    protected override async Task SendNotificationAsync(
        string method,
        CancellationToken cancellationToken,
        string? bearerToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, EndpointUri)
        {
            Content = new StringContent(McpJsonRpc.CreateNotification(method), Encoding.UTF8, "application/json")
        };
        ApplyProtocolHeaders(request, bearerToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        RememberSession(response);
        if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Accepted or HttpStatusCode.NoContent)
        {
            return;
        }

        var body = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
        throw Connection(McpJsonRpc.FormatHttpFailure(response, body));
    }

    private void RememberSession(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(McpJsonRpc.SessionHeader, out var values))
        {
            var sessionId = values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                SessionId = sessionId;
            }
        }
    }

    private async Task<JsonElement> ReadResultAsync(
        HttpResponseMessage response,
        int id,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var failure = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
            throw Connection(McpJsonRpc.FormatHttpFailure(response, failure));
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (mediaType.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase))
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await foreach (var frame in SseEventReader.ReadAsync(stream, cancellationToken))
            {
                if (!McpJsonRpc.TryParseMessage(frame.Data, out var message) || message.Id != id)
                {
                    continue;
                }

                if (message.Error is not null)
                {
                    throw Connection(message.Error);
                }

                if (message.HasResult)
                {
                    return message.Result;
                }
            }

            throw Connection($"MCP server closed the event stream before a response to request {id}.");
        }

        var body = await McpJsonRpc.ReadBodyAsync(response, cancellationToken);
        if (!McpJsonRpc.TryParseMessage(body, out var direct) || direct.Id != id)
        {
            throw Connection("MCP server returned a response that was not JSON-RPC: " + McpJsonRpc.Snippet(body));
        }

        if (direct.Error is not null)
        {
            throw Connection(direct.Error);
        }

        if (!direct.HasResult)
        {
            throw Connection("MCP server returned a JSON-RPC response without a result.");
        }

        return direct.Result;
    }
}
