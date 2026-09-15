using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.Registry;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

public class MCPToolRegistryPersistTests
{
    [Fact]
    public async Task ResolveAsync_persists_last_connection_on_hot_path_failure()
    {
        var server = new MCPServer(
            Guid.NewGuid(),
            "demo-server",
            "Demo",
            MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");

        var serverRepository = Substitute.For<IMCPServerRepository>();
        serverRepository
            .FindByKeyAsync("demo-server", Arg.Any<CancellationToken>())
            .Returns(server);

        var registry = CreateRegistry(serverRepository);

        await registry.ResolveAsync(new[] { "external.demo-server.ping" });

        server.LastConnectionError.ShouldNotBeNull();
        await serverRepository.Received(1).UpdateAsync(
            server,
            autoSave: true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestServerConnectionAsync_does_not_persist_last_connection()
    {
        var server = new MCPServer(
            Guid.NewGuid(),
            "demo-server",
            "Demo",
            MCPTransportType.HTTP);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");

        var serverRepository = Substitute.For<IMCPServerRepository>();
        var registry = CreateRegistry(serverRepository);

        await registry.TestServerConnectionAsync(server);

        await serverRepository.DidNotReceive().UpdateAsync(
            Arg.Any<MCPServer>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    private static MCPToolRegistry CreateRegistry(IMCPServerRepository serverRepository)
    {
        var discovery = Substitute.For<IInternalToolDiscoveryService>();
        discovery.DiscoverToolsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<IMCPTool>());

        return new MCPToolRegistry(
            Substitute.For<IMCPCatalogCache>(),
            discovery,
            serverRepository,
            Substitute.For<IHttpClientFactory>(),
            new ServiceCollection().BuildServiceProvider(),
            NullLoggerFactory.Instance,
            NullLogger<MCPToolRegistry>.Instance);
    }
}
