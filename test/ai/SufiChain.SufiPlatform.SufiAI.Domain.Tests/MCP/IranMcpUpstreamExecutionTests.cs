using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.External;
using SufiChain.SufiPlatform.SufiAI.MCP.IranMcp;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

public class IranMcpUpstreamExecutionTests
{
    [Fact]
    public void Known_not_ready_servers_are_unavailable()
    {
        IranMcpUnavailableServers.IsUnavailable("okala-mcp").ShouldBeTrue();
        IranMcpUnavailableServers.IsUnavailable("divar-mcp").ShouldBeTrue();
        IranMcpUnavailableServers.IsUnavailable("payping-mcp").ShouldBeTrue();
        IranMcpUnavailableServers.IsUnavailable("bale-mcp").ShouldBeTrue();
        IranMcpUnavailableServers.IsUnavailable("pasavard-mcp").ShouldBeTrue();
        IranMcpUnavailableServers.IsUnavailable("swapwallet-mcp").ShouldBeTrue();
        IranMcpUnavailableServers.IsUnavailable("tgju-mcp").ShouldBeFalse();
    }

    [Fact]
    public async Task Allow_upstream_setting_defaults_off()
    {
        var gate = new IranMcpUpstreamExecutionGate(
            new FakeSettings(null),
            NullLogger<IranMcpUpstreamExecutionGate>.Instance);
        (await gate.IsEnabledAsync()).ShouldBeFalse();
    }

    [Fact]
    public async Task Successful_call_against_fake_mcp_server()
    {
        using var server = new FakeMcpHttpServer(delayMs: 0, unavailable: false);
        server.Start();
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var client = new StreamableHttpTransportClient(
            server.Endpoint,
            http,
            NullLogger<StreamableHttpTransportClient>.Instance,
            requestTimeout: TimeSpan.FromSeconds(5));
        await client.ConnectAsync();
        var result = await client.CallToolAsync(
            "tj_get_prices",
            new Dictionary<string, object?> { ["q"] = "gold" },
            CancellationToken.None,
            bearerToken: "user-token");
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task Unavailable_fake_server_returns_clean_tool_result()
    {
        using var server = new FakeMcpHttpServer(delayMs: 0, unavailable: true);
        server.Start();
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var client = new StreamableHttpTransportClient(
            server.Endpoint,
            http,
            NullLogger<StreamableHttpTransportClient>.Instance,
            requestTimeout: TimeSpan.FromSeconds(5));
        await client.ConnectAsync();
        var result = await client.CallToolAsync(
            "ok_search",
            new Dictionary<string, object?>(),
            CancellationToken.None,
            bearerToken: "user-token");
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("service unavailable");
    }

    [Fact]
    public async Task Timeout_against_fake_mcp_server_is_cancelled()
    {
        using var server = new FakeMcpHttpServer(delayMs: 2500, unavailable: false);
        server.Start();
        using var http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(200) };
        using var client = new StreamableHttpTransportClient(
            server.Endpoint,
            http,
            NullLogger<StreamableHttpTransportClient>.Instance,
            requestTimeout: TimeSpan.FromMilliseconds(200));
        await Should.ThrowAsync<Exception>(async () => await client.ConnectAsync());
    }

    private sealed class FakeSettings : ISettingProvider
    {
        private readonly string? _value;

        public FakeSettings(string? value) => _value = value;

        public Task<string?> GetOrNullAsync(string name)
            => Task.FromResult(name == IranMcpSettings.AllowUpstreamExecution ? _value : null);

        public Task<List<SettingValue>> GetAllAsync(string[] names)
            => Task.FromResult(new List<SettingValue>());

        public Task<List<SettingValue>> GetAllAsync()
            => Task.FromResult(new List<SettingValue>());
    }

    private sealed class FakeMcpHttpServer : IDisposable
    {
        private readonly HttpListener _listener = new();
        private readonly int _delayMs;
        private readonly bool _unavailable;
        private CancellationTokenSource? _cts;

        public FakeMcpHttpServer(int delayMs, bool unavailable)
        {
            _delayMs = delayMs;
            _unavailable = unavailable;
            var port = GetFreePort();
            Endpoint = $"http://127.0.0.1:{port}/mcp";
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        public string Endpoint { get; }

        public void Start()
        {
            _listener.Start();
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => LoopAsync(_cts.Token));
        }

        private async Task LoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync().WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                _ = Task.Run(() => HandleAsync(context), CancellationToken.None);
            }
        }

        private async Task HandleAsync(HttpListenerContext context)
        {
            try
            {
                if (_delayMs > 0)
                {
                    await Task.Delay(_delayMs);
                }

                if (context.Request.HttpMethod == "DELETE")
                {
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                    return;
                }

                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                var body = await reader.ReadToEndAsync();
                using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
                var root = document.RootElement;
                var method = root.TryGetProperty("method", out var methodEl) ? methodEl.GetString() : null;
                var id = root.TryGetProperty("id", out var idEl) ? idEl.Clone() : default;

                if (method == "notifications/initialized")
                {
                    context.Response.StatusCode = 202;
                    context.Response.Close();
                    return;
                }

                object response;
                if (method == "initialize")
                {
                    context.Response.Headers["Mcp-Session-Id"] = Guid.NewGuid().ToString("N");
                    response = new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            protocolVersion = "2025-03-26",
                            capabilities = new { },
                            serverInfo = new { name = "fake", version = "1.0" }
                        }
                    };
                }
                else if (method == "tools/call")
                {
                    response = new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            content = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = _unavailable ? "service unavailable: okala-mcp" : "from-fake"
                                }
                            },
                            isError = _unavailable
                        }
                    };
                }
                else
                {
                    response = new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new { tools = Array.Empty<object>() }
                    };
                }

                var json = JsonSerializer.Serialize(response);
                var bytes = Encoding.UTF8.GetBytes(json);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
            catch
            {
                try { context.Response.Abort(); } catch { /* ignore */ }
            }
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
                _listener.Stop();
                _listener.Close();
            }
            catch
            {
                // ignore shutdown races
            }
        }

        private static int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
