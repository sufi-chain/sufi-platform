using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// sufi-chain#384 slice 6b: one connected transport client handed out for exactly one tool call.
/// <para>
/// <see cref="Owned"/> is true for a <c>UserScoped</c> server: that client exists for this call and this
/// user only, and <see cref="IMCPTransportClientProvider.ReleaseAsync"/> disconnects and disposes it. For a
/// <c>TenantLevel</c> server the lease wraps the shared, cached client and releasing it is a no-op.
/// </para>
/// </summary>
public sealed class MCPClientLease
{
    public MCPClientLease(IMCPTransportClient client, string? bearerToken, bool owned)
    {
        Client = client;
        BearerToken = bearerToken;
        Owned = owned;
    }

    /// <summary>The connected client to use for this call.</summary>
    public IMCPTransportClient Client { get; }

    /// <summary>Short-lived per-user token for a <c>UserScoped</c> server; null otherwise.</summary>
    public string? BearerToken { get; }

    /// <summary>True when this lease owns the client and the caller must release it.</summary>
    public bool Owned { get; }
}

/// <summary>
/// sufi-chain#384 slice 6b: owns the transport clients for external MCP servers. External tools and the
/// registry both go through it, so the sharing rule lives in exactly one place.
/// </summary>
public interface IMCPTransportClientProvider
{
    /// <summary>
    /// Acquires a connected client for one call, plus the per-user token when the server is
    /// <c>UserScoped</c>. Throws <c>AI:MCPUserScopedRequiresUser</c> when the server is user-scoped and the
    /// call carries no signed-in user, so nothing is dialled.
    /// </summary>
    Task<MCPClientLease> AcquireAsync(
        MCPServer server,
        AgentCallContext? call,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a lease. Disconnects and disposes the client only when the lease owns it; a shared
    /// tenant-level client outlives the call.
    /// </summary>
    Task ReleaseAsync(MCPClientLease lease, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists a server's tools for the catalog/discovery. Identity-free by design: a <c>UserScoped</c> server
    /// is listed without a token, because only execution knows the user.
    /// </summary>
    Task<List<MCPServerToolDefinition>> ListToolsAsync(
        MCPServer server,
        CancellationToken cancellationToken = default);

    /// <summary>Opens and immediately closes a client, for the admin "test connection" action.</summary>
    Task<bool> TestConnectionAsync(MCPServer server, CancellationToken cancellationToken = default);

    /// <summary>Drops every cached tenant-level client (registry refresh).</summary>
    Task DisconnectAllAsync(CancellationToken cancellationToken = default);
}
