using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.External;
using SufiChain.SufiPlatform.SufiAI.MCP.Registry;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Security.Encryption;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#557: stdio handshake, environment injection, the HTTP gateway in front of stdio,
/// and the admin tool-effect policy. The fixture speaks MCP JSON-RPC. No real secret is used.
/// </summary>
public class McpStdioHandshakeAndPolicyTests
{
    [Fact]
    public async Task Stdio_initialize_is_accepted_and_secrets_stay_out_of_args_and_logs()
    {
        var secret = "fx-" + Guid.NewGuid().ToString("N");
        var parentLeak = "parent-" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable("SUFI_PARENT_API_TOKEN", parentLeak);
        var logger = new ListLogger();
        try
        {
            using var client = new StdioTransportClient(
                "python3",
                new[] { FixturePath(), "stdio" },
                logger,
                new Dictionary<string, string> { ["SUFI_MCP_FIXTURE_TOKEN"] = secret });

            await client.ConnectAsync();
            var tools = await client.ListToolsAsync();
            var result = await client.CallToolAsync("echo_env", new Dictionary<string, object?>());
            await client.DisconnectAsync();

            tools.Select(tool => tool.Name).OrderBy(name => name).ShouldBe(new[] { "echo_env", "write_note" });
            tools.Single(tool => tool.Name == "echo_env").ReadOnlyHint.ShouldBe(true);
            tools.Single(tool => tool.Name == "write_note").ReadOnlyHint.ShouldBeNull();

            using var payload = JsonDocument.Parse(Text(result));
            var root = payload.RootElement;
            root.GetProperty("present").GetBoolean().ShouldBeTrue();
            root.GetProperty("token").GetString().ShouldBe(secret);
            root.GetProperty("argvHasToken").GetBoolean().ShouldBeFalse();
            root.GetProperty("parentLeak").GetBoolean().ShouldBeFalse();

            var logs = string.Join('\n', logger.Lines);
            logs.ShouldNotContain(secret);
            logs.ShouldNotContain(parentLeak);
            logs.ShouldContain("2025-03-26");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SUFI_PARENT_API_TOKEN", null);
        }
    }

    [Fact]
    public async Task Stdio_refuses_to_put_a_configured_secret_in_arguments()
    {
        var secret = "fx-" + Guid.NewGuid().ToString("N");
        using var client = new StdioTransportClient(
            "python3",
            new[] { secret },
            new ListLogger(),
            new Dictionary<string, string> { ["SUFI_MCP_FIXTURE_TOKEN"] = secret });

        var error = await Should.ThrowAsync<global::Volo.Abp.BusinessException>(() => client.ConnectAsync());

        error.Data["Error"]!.ToString().ShouldContain("arguments");
        error.Data["Error"]!.ToString().ShouldNotContain(secret);
    }

    [Fact]
    public async Task Streamable_http_gateway_fronts_the_stdio_fixture()
    {
        var transcript = Path.Combine(Path.GetTempPath(), "mcp-557-" + Guid.NewGuid().ToString("N") + ".log");
        using var gateway = StartGateway(transcript);
        using var http = new HttpClient();
        using var client = new StreamableHttpTransportClient(
            $"http://127.0.0.1:{gateway.Port}/mcp",
            http,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<StreamableHttpTransportClient>.Instance);

        await client.ConnectAsync();
        var tools = await client.ListToolsAsync(CancellationToken.None, "user-token");
        var result = await client.CallToolAsync("write_note", new Dictionary<string, object?> { ["text"] = "hi" });
        await client.DisconnectAsync();

        tools.ShouldContain(tool => tool.Name == "echo_env" && tool.ReadOnlyHint == true);
        JsonSerializer.Serialize(result.Result).ShouldContain("wrote");

        var calls = File.ReadAllLines(transcript).Select(line => JsonDocument.Parse(line)).ToList();
        calls.ShouldAllBe(call => call.RootElement.GetProperty("path").GetString() == "/mcp");
        var initialize = calls.Single(call => call.RootElement.GetProperty("body").GetString()!.Contains("\"method\":\"initialize\"", StringComparison.Ordinal));
        var body = initialize.RootElement.GetProperty("body").GetString()!;
        body.ShouldContain("2025-03-26");
        body.ShouldContain("capabilities");
        body.ShouldContain("clientInfo");
        initialize.RootElement.GetProperty("authorization").ValueKind.ShouldBe(JsonValueKind.Null);
        calls.Single(call => call.RootElement.GetProperty("body").GetString()!.Contains("\"method\":\"tools/list\"", StringComparison.Ordinal))
            .RootElement.GetProperty("authorization").GetString().ShouldBe("Bearer user-token");
    }

    [Fact]
    public async Task Admin_effect_wins_and_readOnlyHint_does_not()
    {
        McpToolEffectPolicy.Resolve(null).ShouldBe(AgentActionEffect.Write);
        McpToolEffectPolicy.Resolve(AgentActionEffect.Read).ShouldBe(AgentActionEffect.Read);

        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("http://mcp.test/mcp");
        server.SetAdminToolEffect("lookup", AgentActionEffect.Read);

        var provider = Substitute.For<IMCPTransportClientProvider>();
        provider.ListToolsAsync(server, Arg.Any<CancellationToken>()).Returns(new List<MCPServerToolDefinition>
        {
            new() { Name = "lookup", Description = "Lookup", ParameterSchema = "{}", ReadOnlyHint = false },
            new() { Name = "pay", Description = "Pay", ParameterSchema = "{}", ReadOnlyHint = true }
        });

        var registry = Registry(provider, new AITransportOptions(), server);
        var resolved = await registry.ResolveAsync(new[] { "external.demo.lookup", "external.demo.pay" });

        EffectOf(resolved, "external.demo.lookup").ShouldBe(AgentActionEffect.Read);
        EffectOf(resolved, "external.demo.pay").ShouldBe(AgentActionEffect.Write);
        resolved.Tools.Single(tool => tool.Name == "external.demo.pay")
            .ShouldBeAssignableTo<IMCPToolCatalogAnnotation>()!
            .ReadOnlyHint.ShouldBe(true);
    }

    [Fact]
    public async Task Discovery_uses_the_configured_timeout_instead_of_a_hardcoded_15_seconds()
    {
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("http://mcp.test/mcp");
        var provider = Substitute.For<IMCPTransportClientProvider>();
        provider.ListToolsAsync(server, Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30), call.Arg<CancellationToken>());
                return new List<MCPServerToolDefinition>();
            });

        var registry = Registry(provider, new AITransportOptions { RequestTimeoutSeconds = 1 }, server);
        var watch = Stopwatch.StartNew();
        var resolved = await registry.ResolveAsync(new[] { "external.demo.lookup" });
        watch.Stop();

        watch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(8));
        resolved.Tools.ShouldBeEmpty();
        resolved.Diagnostics.Single().Message.ShouldContain("Timed out");
        Source("MCPToolRegistry.cs").ShouldNotContain("ExternalDiscoveryTimeout");
        Source("MCPCatalogCache.cs").ShouldNotContain("FromSeconds(15)");
    }

    [Fact]
    public void Environment_values_are_encrypted_and_key_names_do_not_reveal_them()
    {
        var secret = "fx-" + Guid.NewGuid().ToString("N");
        var server = new MCPServer(Guid.NewGuid(), "demo", "Demo", MCPTransportType.STDIO);
        server.ConfigureStdio("python3", null);
        var encryptor = new PrefixEncryptor();

        McpServerEnvironment.Apply(server, encryptor, new Dictionary<string, string?> { ["PROVIDER_TOKEN"] = secret });

        var stored = server.GetProperty<string>(McpServerEnvironment.PropertyName);
        stored.ShouldNotContain(secret);
        McpServerEnvironment.KeyNames(server).ShouldBe(new[] { "PROVIDER_TOKEN" });
        McpServerEnvironment.Decrypt(server, encryptor)["PROVIDER_TOKEN"].ShouldBe(secret);

        McpServerEnvironment.Apply(server, encryptor, new Dictionary<string, string?>());
        McpServerEnvironment.KeyNames(server).ShouldBeEmpty();
    }

    private static AgentActionEffect EffectOf(MCPToolResolutionResult result, string name)
    {
        return result.Tools.Single(tool => tool.Name == name).ShouldBeAssignableTo<IMCPToolEffect>()!.Effect;
    }

    private static MCPToolRegistry Registry(
        IMCPTransportClientProvider provider,
        AITransportOptions options,
        MCPServer server)
    {
        var discovery = Substitute.For<IInternalToolDiscoveryService>();
        discovery.DiscoverToolsAsync(Arg.Any<CancellationToken>()).Returns(new List<IMCPTool>());
        var servers = Substitute.For<IMCPServerRepository>();
        servers.FindByKeyAsync("demo", Arg.Any<CancellationToken>()).Returns(server);
        servers.UpdateAsync(Arg.Any<MCPServer>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<MCPServer>());

        return new MCPToolRegistry(
            Substitute.For<IMCPCatalogCache>(),
            discovery,
            servers,
            new ServiceCollection().BuildServiceProvider(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MCPToolRegistry>.Instance,
            auditor: null,
            clientProvider: provider,
            transportOptions: Options.Create(options));
    }

    private static string FixturePath()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "MCP", "Fixtures", "standard_mcp_fixture.py");
        File.Exists(path).ShouldBeTrue(path);
        return path;
    }

    private static string Text(MCPServerToolResult result)
    {
        result.Success.ShouldBeTrue(result.ErrorMessage);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result.Result));
        return document.RootElement.GetProperty("content")[0].GetProperty("text").GetString()!;
    }

    private static GatewayProcess StartGateway(string transcript)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "python3",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        process.StartInfo.ArgumentList.Add(FixturePath());
        process.StartInfo.ArgumentList.Add("http");
        process.StartInfo.ArgumentList.Add("0");
        process.StartInfo.ArgumentList.Add(transcript);
        process.Start();
        _ = process.StandardError.ReadToEndAsync();
        var line = process.StandardOutput.ReadLine();
        line.ShouldNotBeNull();
        line.ShouldStartWith("PORT ");
        return new GatewayProcess(process, int.Parse(line["PORT ".Length..]));
    }

    private static string Source(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var matches = dir.GetFiles(fileName, SearchOption.AllDirectories);
            var match = matches.FirstOrDefault(file => file.FullName.Contains($"{Path.DirectorySeparatorChar}MCP{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
            if (match != null)
            {
                return File.ReadAllText(match.FullName);
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not find {fileName}");
    }

    private sealed class GatewayProcess : IDisposable
    {
        private readonly Process _process;

        public GatewayProcess(Process process, int port)
        {
            _process = process;
            Port = port;
        }

        public int Port { get; }

        public void Dispose()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
                // Already gone.
            }

            _process.Dispose();
        }
    }

    private sealed class ListLogger : ILogger<StdioTransportClient>
    {
        public List<string> Lines { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Lines.Add(formatter(state, exception));
            if (exception != null)
            {
                Lines.Add(exception.ToString());
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose()
            {
            }
        }
    }

    private sealed class PrefixEncryptor : IStringEncryptionService
    {
        public string? Encrypt(string? plainText, string? passPhrase = null, byte[]? salt = null)
        {
            return plainText is null ? null : "enc:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
        }

        public string? Decrypt(string? cipherText, string? passPhrase = null, byte[]? salt = null)
        {
            if (cipherText is null || !cipherText.StartsWith("enc:", StringComparison.Ordinal))
            {
                return null;
            }

            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cipherText["enc:".Length..]));
        }
    }
}
