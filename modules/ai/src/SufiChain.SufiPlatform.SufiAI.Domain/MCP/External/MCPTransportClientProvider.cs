using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Auth;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.External;

/// <summary>
/// sufi-chain#384 slice 6b: owns the transport clients for external MCP servers.
/// <para>
/// <b>TenantLevel</b> servers keep the historical behaviour: one connected client per server, shared by
/// every caller in that tenant, cached for the lifetime of the process.
/// </para>
/// <para>
/// <b>UserScoped</b> servers never share a client. Every call gets a freshly connected client and a
/// short-lived token minted for the calling user, and that client is disconnected and disposed when the
/// call finishes. That is the whole point of the binding mode: a socket that carried user A's credentials
/// can never answer user B's next call.
/// </para>
/// <para>
/// Catalog/discovery reads pass no user identity, so a user-scoped server is listed without a token. It
/// can only expose its tool metadata that way; execution always carries the identity.
/// </para>
/// </summary>
public class MCPTransportClientProvider : IMCPTransportClientProvider, ISingletonDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MCPTransportClientProvider> _logger;
    private readonly IMcpUserTokenService? _userTokenService;

    // TenantLevel only. Keyed by server id, which is already tenant-scoped in the database.
    private readonly Dictionary<Guid, IMCPTransportClient> _sharedClients = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public MCPTransportClientProvider(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        ILogger<MCPTransportClientProvider> logger,
        IMcpUserTokenService? userTokenService = null)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        _logger = logger;
        _userTokenService = userTokenService;
    }

    public async Task<MCPClientLease> AcquireAsync(
        MCPServer server,
        AgentCallContext? call,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);

        if (server.BindingMode == MCPBindingMode.UserScoped)
        {
            return await AcquireUserScopedAsync(server, call, cancellationToken);
        }

        var shared = await GetOrCreateSharedClientAsync(server, cancellationToken);
        return new MCPClientLease(shared, bearerToken: null, owned: false);
    }

    public async Task ReleaseAsync(MCPClientLease lease, CancellationToken cancellationToken = default)
    {
        if (lease is null || !lease.Owned)
        {
            // A shared tenant-level client outlives the call.
            return;
        }

        try
        {
            await lease.Client.DisconnectAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Error disconnecting a user-scoped MCP client");
        }
        finally
        {
            lease.Client.Dispose();
        }
    }

    /// <inheritdoc />
    public async Task<List<MCPServerToolDefinition>> ListToolsAsync(
        MCPServer server,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);

        if (server.BindingMode == MCPBindingMode.UserScoped)
        {
            using var client = CreateClient(server);
            await client.ConnectAsync(cancellationToken);
            return await client.ListToolsAsync(cancellationToken);
        }

        var shared = await GetOrCreateSharedClientAsync(server, cancellationToken);
        return await shared.ListToolsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(MCPServer server, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);

        using var client = CreateClient(server);
        try
        {
            await client.ConnectAsync(cancellationToken);
            return client.IsConnected;
        }
        finally
        {
            await client.DisconnectAsync(CancellationToken.None);
        }
    }

    /// <inheritdoc />
    public async Task DisconnectAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            foreach (var client in _sharedClients.Values)
            {
                try
                {
                    await client.DisconnectAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(exception, "Error disconnecting a cached MCP client on refresh");
                }

                client.Dispose();
            }

            _sharedClients.Clear();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<MCPClientLease> AcquireUserScopedAsync(
        MCPServer server,
        AgentCallContext? call,
        CancellationToken cancellationToken)
    {
        if (call is null || call.IsAnonymous)
        {
            // No identity, no user-scoped call. Refused before anything is dialled.
            throw new BusinessException(AIErrorCodes.MCPUserScopedRequiresUser)
                .WithData("ServerKey", server.Key);
        }

        var token = _userTokenService?.Issue(call, server.Id);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BusinessException(AIErrorCodes.MCPUserTokenNotPermitted)
                .WithData("ServerKey", server.Key);
        }

        // Never cached: this client exists for one user and one call only.
        var client = CreateClient(server);
        try
        {
            await client.ConnectAsync(cancellationToken);
        }
        catch
        {
            client.Dispose();
            throw;
        }

        return new MCPClientLease(client, token, owned: true);
    }

    private async Task<IMCPTransportClient> GetOrCreateSharedClientAsync(
        MCPServer server,
        CancellationToken cancellationToken)
    {
        if (_sharedClients.TryGetValue(server.Id, out var existing) && existing.IsConnected)
        {
            return existing;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_sharedClients.TryGetValue(server.Id, out existing) && existing.IsConnected)
            {
                return existing;
            }

            var client = CreateClient(server);
            await client.ConnectAsync(cancellationToken);
            _sharedClients[server.Id] = client;

            _logger.LogDebug("Created and connected a shared MCP client for server {Key}", server.Key);
            return client;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Builds a transport client for a server. Virtual so a test (or a host with a custom transport) can
    /// substitute the transport without reimplementing the sharing rules.
    /// </summary>
    protected virtual IMCPTransportClient CreateClient(MCPServer server)
    {
        return server.TransportType switch
        {
            MCPTransportType.STDIO => new StdioTransportClient(
                Check.NotNullOrWhiteSpace(server.Command, nameof(server.Command)),
                string.IsNullOrWhiteSpace(server.ArgumentsJson)
                    ? Array.Empty<string>()
                    : JsonSerializer.Deserialize<string[]>(server.ArgumentsJson) ?? Array.Empty<string>(),
                _loggerFactory.CreateLogger<StdioTransportClient>()),
            MCPTransportType.SSE => new SSETransportClient(
                Check.NotNullOrWhiteSpace(server.Endpoint, nameof(server.Endpoint)),
                _httpClientFactory.CreateClient(),
                _loggerFactory.CreateLogger<SSETransportClient>()),
            MCPTransportType.HTTP => throw new BusinessException(AIErrorCodes.MCPHttpTransportNotImplemented),
            _ => throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
        };
    }
}
