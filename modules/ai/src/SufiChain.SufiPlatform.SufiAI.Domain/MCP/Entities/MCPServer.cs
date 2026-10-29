using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Entities;

/// <summary>
/// Represents an external MCP server configuration.
/// </summary>
public class MCPServer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    
    /// <summary>
    /// Immutable server key (unique per tenant).
    /// Used in qualified external tool names.
    /// </summary>
    public string Key { get; protected set; } = string.Empty;
    
    /// <summary>
    /// Mutable display name.
    /// </summary>
    public string Name { get; protected set; } = string.Empty;
    
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
    /// Last successful connection timestamp.
    /// </summary>
    public DateTime? LastConnectedAt { get; protected set; }
    
    /// <summary>
    /// Last connection error message.
    /// </summary>
    public string? LastConnectionError { get; protected set; }

    /// <summary>
    /// sufi-chain#384 slice 6b: who the server's credential belongs to. Defaults to
    /// <see cref="MCPBindingMode.TenantLevel"/> so every pre-existing row keeps its behaviour.
    /// A <see cref="MCPBindingMode.UserScoped"/> server receives a short-lived token per user and never
    /// shares a transport client between users.
    /// </summary>
    public MCPBindingMode BindingMode { get; protected set; } = MCPBindingMode.TenantLevel;

    protected MCPServer() { }
    
    public MCPServer(
        Guid id,
        string key,
        string name,
        MCPTransportType transportType,
        Guid? tenantId = null
    ) : base(id)
    {
        SetKey(key);
        SetName(name);
        TransportType = transportType;
        TenantId = tenantId;
        IsEnabled = true;
    }

    /// <summary>
    /// sufi-chain#384 slice 6b: sets who the server acts for.
    /// A <see cref="MCPBindingMode.UserScoped"/> server cannot run over STDIO: STDIO has no header or
    /// credential channel we are willing to inject, so the per-user token could not reach it. The
    /// combination is rejected here rather than failing at call time.
    /// </summary>
    public void SetBindingMode(MCPBindingMode bindingMode)
    {
        if (bindingMode == MCPBindingMode.UserScoped && TransportType == MCPTransportType.STDIO)
        {
            throw new BusinessException(AIErrorCodes.MCPUserScopedStdioNotSupported)
                .WithData("Key", Key)
                .WithData("TransportType", TransportType)
                .WithData("BindingMode", bindingMode);
        }

        BindingMode = bindingMode;
    }

    private void SetKey(string key)
    {
        Key = Check.NotNullOrWhiteSpace(key, nameof(key));
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
    
    public void Enable() => IsEnabled = true;
    
    public void Disable() => IsEnabled = false;
    
    /// <summary>
    /// sufi-chain#557: replaces the transport, binding, and the settings that belong to that transport.
    /// The final combination is checked before anything is written, so a rejected edit leaves the
    /// stored server unchanged. User-scoped HTTP can become tenant STDIO, and STDIO can become a
    /// user-scoped HTTP server; user-scoped STDIO stays refused.
    /// </summary>
    public void ApplyConfiguration(
        MCPTransportType transportType,
        MCPBindingMode bindingMode,
        string? endpoint,
        string? command,
        string? argumentsJson)
    {
        if (bindingMode == MCPBindingMode.UserScoped && transportType == MCPTransportType.STDIO)
        {
            throw new BusinessException(AIErrorCodes.MCPUserScopedStdioNotSupported)
                .WithData("Key", Key)
                .WithData("TransportType", transportType)
                .WithData("BindingMode", bindingMode);
        }

        if (transportType == MCPTransportType.STDIO)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                    .WithData("Error", "STDIO server is missing command");
            }
        }
        else if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("Error", $"{transportType} server is missing endpoint");
        }

        // Leave STDIO before applying a user-scoped binding, and drop a user-scoped binding
        // before entering STDIO. The intermediate pair would be rejected by SetBindingMode.
        if (transportType != MCPTransportType.STDIO && TransportType == MCPTransportType.STDIO)
        {
            SetTransport(transportType);
            SetBindingMode(bindingMode);
        }
        else
        {
            SetBindingMode(bindingMode);
            SetTransport(transportType);
        }

        if (transportType == MCPTransportType.STDIO)
        {
            ConfigureStdio(command!, argumentsJson);
        }
        else
        {
            ConfigureHttpEndpoint(endpoint!);
        }
    }

    public void UpdateLastConnection(bool success, string? errorMessage = null)
    {
        if (success)
        {
            LastConnectedAt = DateTime.UtcNow;
            LastConnectionError = null;
        }
        else
        {
            LastConnectionError = errorMessage is { Length: > MaxConnectionErrorLength }
                ? errorMessage[..MaxConnectionErrorLength]
                : errorMessage;
        }
    }

    private const int MaxConnectionErrorLength = 1024;

    private void SetTransport(MCPTransportType transportType)
    {
        if (transportType == TransportType)
        {
            return;
        }

        TransportType = transportType;
        if (transportType == MCPTransportType.STDIO)
        {
            Endpoint = null;
        }
        else
        {
            Command = null;
            ArgumentsJson = null;
        }
    }
}
