using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        server.LastConnectionError.ShouldNotContain("Exception of type");
        server.LastConnectionError.ShouldContain(AIErrorCodes.MCPHttpTransportNotImplemented);
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

        // sufi-chain#384 slice 6b: the registry no longer owns transport clients; the provider does.
        // This substitute stands in for a transport failure. The real HTTP and SSE handshakes are covered
        // by McpStandardTransportTests.
        var clientProvider = Substitute.For<IMCPTransportClientProvider>();
        clientProvider
            .TestConnectionAsync(Arg.Any<MCPServer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(
                new BusinessException(AIErrorCodes.MCPHttpTransportNotImplemented)));
        clientProvider
            .ListToolsAsync(Arg.Any<MCPServer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<MCPServerToolDefinition>>(
                new BusinessException(AIErrorCodes.MCPHttpTransportNotImplemented)));

        return new MCPToolRegistry(
            Substitute.For<IMCPCatalogCache>(),
            discovery,
            serverRepository,
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<MCPToolRegistry>.Instance,
            auditor: null,
            clientProvider: clientProvider);
    }
}
