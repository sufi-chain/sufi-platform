using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.External;
using SufiChain.SufiPlatform.SufiAI.MCP.Registry;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#557: in-memory MCP peers. No socket is opened. Streamable HTTP and legacy SSE
/// must speak JSON-RPC, and a failed connect must keep the upstream cause.
/// </summary>
public class McpStandardTransportTests
{
    [Fact]
    public async Task Streamable_http_lists_and_calls_tools_with_the_negotiated_session()
    {
        var handler = new StreamableHttpPeer();
        using var http = new HttpClient(handler, disposeHandler: false);
        using var client = new StreamableHttpTransportClient(
            "http://mcp.test/mcp",
            http,
            NullLogger<StreamableHttpTransportClient>.Instance);

        await client.ConnectAsync();
        var tools = await client.ListToolsAsync(CancellationToken.None, "user-token");
        var result = await client.CallToolAsync(
            "lookup",
            new Dictionary<string, object?> { ["q"] = "hello" },
            CancellationToken.None,
            "user-token");
        await client.DisconnectAsync();

        tools.Select(tool => tool.Name).OrderBy(name => name).ShouldBe(new[] { "lookup", "ping" });
        result.Success.ShouldBeTrue();
        JsonSerializer.Serialize(result.Result).ShouldContain("pong");

        var initialize = handler.Calls.Single(call => call.Body.Contains("\"method\":\"initialize\"", StringComparison.Ordinal));
        initialize.Path.ShouldBe("/mcp");
        initialize.SessionId.ShouldBeNull();
        initialize.Authorization.ShouldBeNull();
        initialize.Body.ShouldContain("2025-03-26");

        var lists = handler.Calls.Where(call => call.Body.Contains("\"method\":\"tools/list\"", StringComparison.Ordinal)).ToList();
        lists.Count.ShouldBe(2);
        lists.ShouldAllBe(call =>
            call.SessionId == "sess-557"
            && call.ProtocolVersion == "2025-06-18"
            && call.Authorization == "Bearer user-token");

        var toolCall = handler.Calls.Single(call => call.Body.Contains("\"method\":\"tools/call\"", StringComparison.Ordinal));
        toolCall.Body.ShouldContain("hello");
        toolCall.SessionId.ShouldBe("sess-557");
        handler.Calls.ShouldContain(call => call.Method == "DELETE" && call.Path == "/mcp");
        AssertNoLegacyRestPaths(handler.Calls);
    }

    [Fact]
    public async Task Sse_uses_the_endpoint_event_and_message_stream()
    {
        var handler = new LegacySsePeer();
        using var http = new HttpClient(handler, disposeHandler: false);
        using var client = new SSETransportClient(
            "http://mcp.test/sse",
            http,
            NullLogger<SSETransportClient>.Instance);

        await client.ConnectAsync();
        client.IsConnected.ShouldBeTrue();
        var tools = await client.ListToolsAsync(CancellationToken.None, "user-token");
        var result = await client.CallToolAsync(
            "lookup",
            new Dictionary<string, object?> { ["q"] = "hello" });

        tools.Select(tool => tool.Name).OrderBy(name => name).ShouldBe(new[] { "lookup", "ping" });
        result.Success.ShouldBeTrue();
        JsonSerializer.Serialize(result.Result).ShouldContain("pong");

        handler.Calls.ShouldContain(call => call.Method == "GET" && call.Path == "/sse");
        handler.Calls.Where(call => call.Method == "POST").ShouldAllBe(call => call.Path == "/messages?sessionId=abc");
        handler.Calls.Single(call => call.Body.Contains("\"method\":\"initialize\"", StringComparison.Ordinal))
            .Authorization.ShouldBeNull();
        handler.Calls.Where(call => call.Body.Contains("\"method\":\"tools/list\"", StringComparison.Ordinal))
            .ShouldAllBe(call => call.Authorization == "Bearer user-token");
        AssertNoLegacyRestPaths(handler.Calls);
    }

    [Fact]
    public async Task Provider_connects_over_both_transports()
    {
        var httpPeer = new StreamableHttpPeer();
        var httpProvider = Provider(httpPeer);
        var httpServer = Server(MCPTransportType.HTTP, "http://mcp.test/mcp");
        var httpTools = await httpProvider.ListToolsAsync(httpServer);
        httpTools.Select(tool => tool.Name).OrderBy(name => name).ShouldBe(new[] { "lookup", "ping" });
        (await httpProvider.TestConnectionAsync(httpServer)).ShouldBeTrue();

        var ssePeer = new LegacySsePeer();
        var sseProvider = Provider(ssePeer);
        var sseServer = Server(MCPTransportType.SSE, "http://mcp.test/sse");
        var sseTools = await sseProvider.ListToolsAsync(sseServer);
        sseTools.Select(tool => tool.Name).OrderBy(name => name).ShouldBe(new[] { "lookup", "ping" });
        AssertNoLegacyRestPaths(ssePeer.Calls);
        await sseProvider.EvictAsync(sseServer.Id);
    }

    [Fact]
    public async Task Evict_drops_the_shared_client_so_the_next_call_dials_again()
    {
        var handler = new StreamableHttpPeer();
        var provider = Provider(handler);
        var server = Server(MCPTransportType.HTTP, "http://mcp.test/mcp");

        await provider.ListToolsAsync(server);
        Count(handler, "\"method\":\"initialize\"").ShouldBe(1);

        await provider.EvictAsync(server.Id);
        await provider.ListToolsAsync(server);

        Count(handler, "\"method\":\"initialize\"").ShouldBe(2);
    }

    [Fact]
    public async Task Http_failure_reaches_the_registry_as_the_upstream_cause()
    {
        var handler = new StreamableHttpPeer
        {
            FailureStatus = HttpStatusCode.NotFound,
            FailureBody = "no mcp here"
        };
        var registry = Registry(Provider(handler));
        var server = Server(MCPTransportType.HTTP, "http://mcp.test/mcp");

        var (ok, error) = await registry.TestServerConnectionAsync(server);

        ok.ShouldBeFalse();
        error.ShouldNotBeNull();
        error.ShouldContain("404");
        error.ShouldContain("no mcp here");
        error.ShouldNotContain("Exception of type");
        AssertNoLegacyRestPaths(handler.Calls);
    }

    [Fact]
    public async Task Sse_failure_names_the_status_and_does_not_call_health()
    {
        var handler = new LegacySsePeer
        {
            FailGet = true,
            FailureBody = "no such sse"
        };
        using var http = new HttpClient(handler, disposeHandler: false);
        using var client = new SSETransportClient(
            "http://mcp.test/sse",
            http,
            NullLogger<SSETransportClient>.Instance);

        var error = await Should.ThrowAsync<global::Volo.Abp.BusinessException>(() => client.ConnectAsync());

        error.Message.ShouldContain("404");
        error.Message.ShouldContain("no such sse");
        error.Message.ShouldNotContain("Exception of type");
        handler.Calls.ShouldContain(call => call.Method == "GET" && call.Path == "/sse");
        AssertNoLegacyRestPaths(handler.Calls);
    }

    [Fact]
    public async Task Registry_reports_the_business_exception_cause_instead_of_its_type_name()
    {
        var provider = Substitute.For<IMCPTransportClientProvider>();
        provider
            .TestConnectionAsync(Arg.Any<MCPServer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(
                new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                    .WithData("Error", "GET /sse/health returned 404")));

        var registry = Registry(provider);
        var (ok, error) = await registry.TestServerConnectionAsync(Server(MCPTransportType.SSE, "http://mcp.test/sse"));

        ok.ShouldBeFalse();
        error.ShouldBe("GET /sse/health returned 404");
    }

    [Fact]
    public async Task Registry_redacts_secrets_in_the_stored_cause()
    {
        var provider = Substitute.For<IMCPTransportClientProvider>();
        provider
            .TestConnectionAsync(Arg.Any<MCPServer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(
                new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                    .WithData("Error", "upstream said bearer secret-token-value was rejected")));

        var registry = Registry(provider);
        var (_, error) = await registry.TestServerConnectionAsync(Server(MCPTransportType.HTTP, "http://mcp.test/mcp"));

        error.ShouldNotBeNull();
        error.ShouldNotContain("secret-token-value");
        error.ShouldContain("[redacted]");
    }

    private static void AssertNoLegacyRestPaths(IEnumerable<RecordedCall> calls)
    {
        foreach (var call in calls)
        {
            call.Path.ShouldNotContain("/health");
            call.Path.ShouldNotContain("/tools");
        }
    }

    private static int Count(StreamableHttpPeer peer, string body) =>
        peer.Calls.Count(call => call.Body.Contains(body, StringComparison.Ordinal));

    private static MCPTransportClientProvider Provider(HttpMessageHandler handler) =>
        new(new SingleHandlerFactory(handler), NullLoggerFactory.Instance, NullLogger<MCPTransportClientProvider>.Instance);

    private static MCPToolRegistry Registry(IMCPTransportClientProvider provider)
    {
        var discovery = Substitute.For<IInternalToolDiscoveryService>();
        discovery.DiscoverToolsAsync(Arg.Any<CancellationToken>()).Returns(new List<IMCPTool>());
        return new MCPToolRegistry(
            Substitute.For<IMCPCatalogCache>(),
            discovery,
            Substitute.For<IMCPServerRepository>(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<MCPToolRegistry>.Instance,
            auditor: null,
            clientProvider: provider);
    }

    private static MCPServer Server(MCPTransportType transport, string endpoint)
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", transport);
        server.ConfigureHttpEndpoint(endpoint);
        return server;
    }

    private sealed class SingleHandlerFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public SingleHandlerFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed record RecordedCall(
        string Method,
        string Path,
        string Body,
        string? SessionId,
        string? ProtocolVersion,
        string? Authorization);

    private sealed class StreamableHttpPeer : HttpMessageHandler
    {
        public const string SessionId = "sess-557";

        public List<RecordedCall> Calls { get; } = new();

        public HttpStatusCode? FailureStatus { get; init; }

        public string? FailureBody { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            var path = request.RequestUri?.PathAndQuery ?? string.Empty;
            Calls.Add(new RecordedCall(
                request.Method.Method,
                path,
                body,
                Header(request, "Mcp-Session-Id"),
                Header(request, "MCP-Protocol-Version"),
                request.Headers.Authorization?.ToString()));

            if (request.Method == HttpMethod.Delete)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent) { RequestMessage = request };
            }

            if (FailureStatus is not null)
            {
                return Text(request, FailureStatus.Value, FailureBody ?? string.Empty);
            }

            if (request.Method != HttpMethod.Post || request.RequestUri?.AbsolutePath != "/mcp")
            {
                return Text(request, HttpStatusCode.NotFound, "unexpected " + request.Method + " " + path);
            }

            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = document.RootElement;
            var method = root.TryGetProperty("method", out var methodElement) ? methodElement.GetString() : null;
            int? id = root.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var parsed) ? parsed : null;

            if (method == "notifications/initialized")
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted) { RequestMessage = request };
            }

            if (method == "initialize")
            {
                var response = Json(request, Result(id, """{"protocolVersion":"2025-06-18","capabilities":{"tools":{}},"serverInfo":{"name":"fake","version":"1"}}"""));
                response.Headers.TryAddWithoutValidation("Mcp-Session-Id", SessionId);
                return response;
            }

            if (method == "tools/list")
            {
                var cursor = Cursor(root);
                var payload = cursor == "p2"
                    ? """{"tools":[{"name":"lookup","description":"Lookup","inputSchema":{"type":"object"}}]}"""
                    : """{"tools":[{"name":"ping","description":"Ping","inputSchema":{"type":"object"}}],"nextCursor":"p2"}""";
                return EventStream(request, Result(id, payload));
            }

            if (method == "tools/call")
            {
                return Json(request, Result(id, """{"content":[{"type":"text","text":"pong"}],"isError":false}"""));
            }

            return Text(request, HttpStatusCode.BadRequest, "unexpected method " + method);
        }
    }

    private sealed class LegacySsePeer : HttpMessageHandler
    {
        public List<RecordedCall> Calls { get; } = new();

        public bool FailGet { get; init; }

        public string? FailureBody { get; init; }

        private PushStream? _stream;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            var path = request.RequestUri?.PathAndQuery ?? string.Empty;
            Calls.Add(new RecordedCall(
                request.Method.Method,
                path,
                body,
                Header(request, "Mcp-Session-Id"),
                Header(request, "MCP-Protocol-Version"),
                request.Headers.Authorization?.ToString()));

            if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/sse")
            {
                if (FailGet)
                {
                    return Text(request, HttpStatusCode.NotFound, FailureBody ?? string.Empty);
                }

                _stream = new PushStream();
                _stream.Enqueue("event: endpoint\ndata: /messages?sessionId=abc\n\n");
                var content = new StreamContent(_stream);
                content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = content,
                    RequestMessage = request
                };
            }

            if (request.Method == HttpMethod.Post && request.RequestUri?.AbsolutePath == "/messages")
            {
                if (_stream is null)
                {
                    return Text(request, HttpStatusCode.BadRequest, "SSE stream is not open");
                }

                using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
                var root = document.RootElement;
                var method = root.TryGetProperty("method", out var methodElement) ? methodElement.GetString() : null;
                if (method == "notifications/initialized")
                {
                    return new HttpResponseMessage(HttpStatusCode.Accepted) { RequestMessage = request };
                }

                int? id = root.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var parsed) ? parsed : null;
                var payload = method switch
                {
                    "initialize" => """{"protocolVersion":"2025-03-26","capabilities":{"tools":{}},"serverInfo":{"name":"fake","version":"1"}}""",
                    "tools/list" => Cursor(root) == "p2"
                        ? """{"tools":[{"name":"lookup","description":"Lookup","inputSchema":{"type":"object"}}]}"""
                        : """{"tools":[{"name":"ping","description":"Ping","inputSchema":{"type":"object"}}],"nextCursor":"p2"}""",
                    "tools/call" => """{"content":[{"type":"text","text":"pong"}],"isError":false}""",
                    _ => null
                };

                if (payload is null)
                {
                    return Text(request, HttpStatusCode.BadRequest, "unexpected method " + method);
                }

                _stream.Enqueue("event: message\ndata: " + Result(id, payload) + "\n\n");
                return new HttpResponseMessage(HttpStatusCode.Accepted) { RequestMessage = request };
            }

            return Text(request, HttpStatusCode.NotFound, "unexpected " + request.Method + " " + path);
        }
    }

    private sealed class PushStream : Stream
    {
        private readonly Channel<byte[]> _chunks = Channel.CreateUnbounded<byte[]>();
        private byte[]? _current;
        private int _offset;

        public void Enqueue(string text) => _chunks.Writer.TryWrite(Encoding.UTF8.GetBytes(text));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).AsTask().GetAwaiter().GetResult();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            while (_current is null || _offset >= _current.Length)
            {
                if (!await _chunks.Reader.WaitToReadAsync(cancellationToken))
                {
                    return 0;
                }

                if (!_chunks.Reader.TryRead(out _current))
                {
                    continue;
                }

                _offset = 0;
            }

            var count = Math.Min(buffer.Length, _current.Length - _offset);
            _current.AsMemory(_offset, count).CopyTo(buffer);
            _offset += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            _chunks.Writer.TryComplete();
            base.Dispose(disposing);
        }
    }

    private static string? Header(HttpRequestMessage request, string name) =>
        request.Headers.TryGetValues(name, out var values)
            ? values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            : null;

    private static string? Cursor(JsonElement root)
    {
        if (!root.TryGetProperty("params", out var parameters)
            || parameters.ValueKind != JsonValueKind.Object
            || !parameters.TryGetProperty("cursor", out var cursor)
            || cursor.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return cursor.GetString();
    }

    private static string Result(int? id, string resultJson) =>
        "{\"jsonrpc\":\"2.0\",\"id\":" + (id?.ToString() ?? "null") + ",\"result\":" + resultJson + "}";

    private static HttpResponseMessage Json(HttpRequestMessage request, string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };

    private static HttpResponseMessage EventStream(HttpRequestMessage request, string json)
    {
        var content = new StringContent("event: message\ndata: " + json + "\n\n", Encoding.UTF8, "text/event-stream");
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content,
            RequestMessage = request
        };
    }

    private static HttpResponseMessage Text(HttpRequestMessage request, HttpStatusCode status, string body) =>
        new(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/plain"),
            RequestMessage = request
        };
}

public class McpServerTransportEditTests
{
    [Fact]
    public void ApplyConfiguration_changes_sse_to_http()
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.SSE);
        server.ConfigureHttpEndpoint("https://old.example/sse");

        server.ApplyConfiguration(
            MCPTransportType.HTTP,
            MCPBindingMode.TenantLevel,
            "https://new.example/mcp",
            command: null,
            argumentsJson: null);

        server.TransportType.ShouldBe(MCPTransportType.HTTP);
        server.Endpoint.ShouldBe("https://new.example/mcp");
        server.Command.ShouldBeNull();
    }

    [Fact]
    public void ApplyConfiguration_moves_stdio_to_user_scoped_http()
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.STDIO);
        server.ConfigureStdio("npx", "[\"server\"]");

        server.ApplyConfiguration(
            MCPTransportType.HTTP,
            MCPBindingMode.UserScoped,
            "https://mcp.example/mcp",
            command: null,
            argumentsJson: null);

        server.TransportType.ShouldBe(MCPTransportType.HTTP);
        server.BindingMode.ShouldBe(MCPBindingMode.UserScoped);
        server.Endpoint.ShouldBe("https://mcp.example/mcp");
        server.Command.ShouldBeNull();
    }

    [Fact]
    public void ApplyConfiguration_refuses_user_scoped_stdio_without_changing_the_server()
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");
        server.SetBindingMode(MCPBindingMode.UserScoped);

        var error = Should.Throw<global::Volo.Abp.BusinessException>(() => server.ApplyConfiguration(
            MCPTransportType.STDIO,
            MCPBindingMode.UserScoped,
            endpoint: null,
            command: "npx",
            argumentsJson: null));

        error.Code.ShouldBe(AIErrorCodes.MCPUserScopedStdioNotSupported);
        server.TransportType.ShouldBe(MCPTransportType.HTTP);
        server.BindingMode.ShouldBe(MCPBindingMode.UserScoped);
        server.Endpoint.ShouldBe("https://mcp.example/mcp");
    }

    [Fact]
    public void ApplyConfiguration_allows_user_scoped_http_to_become_tenant_stdio()
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");
        server.SetBindingMode(MCPBindingMode.UserScoped);

        server.ApplyConfiguration(
            MCPTransportType.STDIO,
            MCPBindingMode.TenantLevel,
            endpoint: null,
            command: "npx",
            argumentsJson: "[\"a\"]");

        server.TransportType.ShouldBe(MCPTransportType.STDIO);
        server.BindingMode.ShouldBe(MCPBindingMode.TenantLevel);
        server.Command.ShouldBe("npx");
        server.ArgumentsJson.ShouldBe("[\"a\"]");
        server.Endpoint.ShouldBeNull();
    }

    [Fact]
    public void UpdateLastConnection_truncates_to_the_column_length()
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");

        server.UpdateLastConnection(false, new string('x', 2000));

        server.LastConnectionError.ShouldNotBeNull();
        server.LastConnectionError.Length.ShouldBe(1024);
    }
}
