using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Client for communicating with external MCP servers via different transports.
/// </summary>
public interface IMCPTransportClient : IDisposable
{
    /// <summary>
    /// Transport type (STDIO, SSE, HTTP).
    /// </summary>
    MCPTransportType TransportType { get; }
    
    /// <summary>
    /// Connect to the MCP server.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disconnect from the MCP server.
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if the client is connected.
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// List available tools from the MCP server.
    /// </summary>
    Task<List<MCPServerToolDefinition>> ListToolsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Call a tool on the MCP server.
    /// </summary>
    Task<MCPServerToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// MCP transport types.
/// </summary>
public enum MCPTransportType
{
    /// <summary>
    /// Standard input/output (process-based).
    /// </summary>
    STDIO,
    
    /// <summary>
    /// Server-Sent Events (HTTP streaming).
    /// </summary>
    SSE,
    
    /// <summary>
    /// HTTP (request/response).
    /// </summary>
    HTTP
}
