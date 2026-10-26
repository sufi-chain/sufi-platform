using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Client for communicating with external MCP servers via different transports.
/// <para>
/// sufi-chain#384 slice 6b: a <c>UserScoped</c> server receives a short-lived per-user token as
/// <c>bearerToken</c> on every request. Transports that cannot carry a token (STDIO) ignore it, and the
/// domain refuses to pair STDIO with a user-scoped binding.
/// </para>
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
    /// <param name="cancellationToken">Turn token.</param>
    /// <param name="bearerToken">
    /// Short-lived per-user token for a <c>UserScoped</c> server; null for a tenant-level server.
    /// </param>
    Task<List<MCPServerToolDefinition>> ListToolsAsync(
        CancellationToken cancellationToken = default,
        string? bearerToken = null);

    /// <summary>
    /// Call a tool on the MCP server.
    /// </summary>
    /// <param name="toolName">Remote tool name on the MCP server.</param>
    /// <param name="parameters">Call arguments.</param>
    /// <param name="cancellationToken">Turn token.</param>
    /// <param name="bearerToken">
    /// Short-lived per-user token for a <c>UserScoped</c> server; null for a tenant-level server.
    /// </param>
    Task<MCPServerToolResult> CallToolAsync(
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default,
        string? bearerToken = null);
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
