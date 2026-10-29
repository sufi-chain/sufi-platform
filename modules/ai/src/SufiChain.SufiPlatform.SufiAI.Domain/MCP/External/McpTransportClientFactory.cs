using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// Builds the transport client for an <see cref="MCPServer"/>. The registry provider and the
/// catalog cache both use this so HTTP, SSE, and STDIO stay on one path.
/// </summary>
public static class McpTransportClientFactory
{
    public static IMCPTransportClient Create(
        MCPServer server,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        AITransportOptions? transportOptions = null)
    {
        ArgumentNullException.ThrowIfNull(server);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var requestTimeout = (transportOptions ?? new AITransportOptions()).GetRequestTimeout();
        return server.TransportType switch
        {
            MCPTransportType.STDIO => new StdioTransportClient(
                Check.NotNullOrWhiteSpace(server.Command, nameof(server.Command)),
                ReadArguments(server.ArgumentsJson),
                loggerFactory.CreateLogger<StdioTransportClient>()),
            MCPTransportType.SSE => new SSETransportClient(
                Check.NotNullOrWhiteSpace(server.Endpoint, nameof(server.Endpoint)),
                httpClientFactory.CreateClient(),
                loggerFactory.CreateLogger<SSETransportClient>(),
                requestTimeout),
            MCPTransportType.HTTP => new StreamableHttpTransportClient(
                Check.NotNullOrWhiteSpace(server.Endpoint, nameof(server.Endpoint)),
                httpClientFactory.CreateClient(),
                loggerFactory.CreateLogger<StreamableHttpTransportClient>(),
                requestTimeout),
            _ => throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("TransportType", server.TransportType)
        };
    }

    private static string[] ReadArguments(string? argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
        {
            return Array.Empty<string>();
        }

        return JsonSerializer.Deserialize<string[]>(argumentsJson) ?? Array.Empty<string>();
    }
}
