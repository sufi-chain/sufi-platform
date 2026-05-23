using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.AIManagement.MCP.Abstractions;

namespace SufiChain.SufiAbp.AIManagement.MCP.Entities;

/// <summary>
/// Represents an external MCP server configuration.
/// </summary>
public class MCPServer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    
    /// <summary>
    /// Server name (unique per workspace).
    /// </summary>
    public string Name { get; protected set; } = string.Empty;
    
    /// <summary>
    /// Workspace ID this server belongs to.
    /// </summary>
    public Guid WorkspaceId { get; protected set; }
    
    /// <summary>
    /// Transport type (STDIO, SSE, HTTP).
    /// </summary>
    public MCPTransportType TransportType { get; protected set; }
    
    /// <summary>
    /// Endpoint URL (for SSE/HTTP) or null for STDIO.
    /// </summary>
    public string? Endpoint { get; protected set; }
    
    /// <summary>
    /// Command to execute (for STDIO transport).
    /// Example: "npx", "node", "python"
    /// </summary>
    public string? Command { get; protected set; }
    
    /// <summary>
    /// Command arguments (for STDIO transport).
    /// Example: ["@modelcontextprotocol/server-filesystem", "/path/to/data"]
    /// </summary>
    public string? ArgumentsJson { get; protected set; }
    
    /// <summary>
    /// Whether the server is enabled.
    /// </summary>
    public bool IsEnabled { get; protected set; }
    
    /// <summary>
    /// Additional metadata (authentication tokens, headers, etc.).
    /// </summary>
    public string? MetadataJson { get; protected set; }
    
    /// <summary>
    /// Last successful connection timestamp.
    /// </summary>
    public DateTime? LastConnectedAt { get; protected set; }
    
    /// <summary>
    /// Last connection error message.
    /// </summary>
    public string? LastConnectionError { get; protected set; }
    
    protected MCPServer() { }
    
    public MCPServer(
        Guid id,
        string name,
        Guid workspaceId,
        MCPTransportType transportType,
        Guid? tenantId = null
    ) : base(id)
    {
        SetName(name);
        WorkspaceId = workspaceId;
        TransportType = transportType;
        TenantId = tenantId;
        IsEnabled = true;
    }
    
    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
    }
    
    public void ConfigureStdio(string command, string? argumentsJson)
    {
        if (TransportType != MCPTransportType.STDIO)
        {
            throw new BusinessException("STDIO_CONFIG_INVALID")
                .WithData("TransportType", TransportType);
        }
        
        Command = Check.NotNullOrWhiteSpace(command, nameof(command));
        ArgumentsJson = argumentsJson;
        Endpoint = null;
    }
    
    public void ConfigureHttpEndpoint(string endpoint)
    {
        if (TransportType == MCPTransportType.STDIO)
        {
            throw new BusinessException("HTTP_CONFIG_INVALID")
                .WithData("TransportType", TransportType);
        }
        
        Endpoint = Check.NotNullOrWhiteSpace(endpoint, nameof(endpoint));
        Command = null;
        ArgumentsJson = null;
    }
    
    public void SetMetadata(string? metadataJson)
    {
        MetadataJson = metadataJson;
    }
    
    public void Enable() => IsEnabled = true;
    
    public void Disable() => IsEnabled = false;
    
    public void UpdateLastConnection(bool success, string? errorMessage = null)
    {
        if (success)
        {
            LastConnectedAt = DateTime.UtcNow;
            LastConnectionError = null;
        }
        else
        {
            LastConnectionError = errorMessage;
        }
    }
}
