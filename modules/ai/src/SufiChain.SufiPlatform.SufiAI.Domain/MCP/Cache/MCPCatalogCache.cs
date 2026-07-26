using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using SufiChain.SufiPlatform.SufiAI.Adapters;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.External;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Cache;

/// <summary>
/// Long-lived distributed cache of the MCP catalog. The only place that performs live
/// external transport discovery. Catalog reads (admin pages, test-chat tool list) always
/// prefer this cache and never block the Blazor circuit on STDIO/SSE for longer than the
/// rebuild lock wait.
/// </summary>
public class MCPCatalogCache : IMCPCatalogCache, ITransientDependency
{
    private const string ExternalPrefix = "external.";
    private static readonly TimeSpan ExternalDiscoveryTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan CacheAbsoluteLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan CacheRebuildLockTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan CacheMissLockWait = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan CacheMissPollInterval = TimeSpan.FromMilliseconds(200);
    private const int CacheMissPollAttempts = 15;

    private readonly IDistributedCache<MCPCatalogCacheItem> _cache;
    private readonly IInternalToolDiscoveryService _internalToolDiscovery;
    private readonly IMCPServerRepository _serverRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MCPCatalogCache> _logger;
    private static readonly SemaphoreSlim RebuildLock = new(1, 1);
    private static int _backgroundRebuildGate;

    public MCPCatalogCache(
        IDistributedCache<MCPCatalogCacheItem> cache,
        IInternalToolDiscoveryService internalToolDiscovery,
        IMCPServerRepository serverRepository,
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory,
        ILogger<MCPCatalogCache> logger)
    {
        _cache = cache;
        _internalToolDiscovery = internalToolDiscovery;
        _serverRepository = serverRepository;
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<MCPCatalogCacheItem> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var item = await _cache.GetAsync(nameof(MCPCatalogCacheItem), token: cancellationToken)
            .ConfigureAwait(false);
        if (item != null)
        {
            return item;
        }

        // NEVER rebuild (STDIO/SSE) on the catalog read path — that freezes Blazor Server
        // circuits. Kick a background rebuild and poll briefly for the entry.
        KickBackgroundRebuild();

        return await WaitForCatalogAsync(cancellationToken).ConfigureAwait(false);
    }

    private void KickBackgroundRebuild()
    {
        if (Interlocked.CompareExchange(ref _backgroundRebuildGate, 1, 0) != 0)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var cache = scope.ServiceProvider.GetRequiredService<IMCPCatalogCache>();
                await cache.RebuildAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background MCP catalog rebuild failed");
            }
            finally
            {
                Interlocked.Exchange(ref _backgroundRebuildGate, 0);
            }
        });
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating MCP catalog cache");
        await _cache.RemoveAsync(nameof(MCPCatalogCacheItem), token: cancellationToken);
    }

    public async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        var acquired = await RebuildLock.WaitAsync(CacheRebuildLockTimeout, cancellationToken);
        if (!acquired)
        {
            _logger.LogWarning(
                "Timed out after {Timeout}s waiting for MCP catalog rebuild lock; skipping rebuild",
                CacheRebuildLockTimeout.TotalSeconds);
            return;
        }

        try
        {
            await RebuildCoreAsync(cancellationToken);
        }
        finally
        {
            RebuildLock.Release();
        }
    }

    private async Task RebuildCoreAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rebuilding MCP catalog cache");
        var overall = Stopwatch.StartNew();

        try
        {
            var item = new MCPCatalogCacheItem { BuiltAtUtc = DateTime.UtcNow };

            item.InternalTools = await DiscoverInternalDescriptorsAsync(cancellationToken);
            _logger.LogDebug("Internal discovery produced {Count} tools in {Elapsed}ms",
                item.InternalTools.Count, overall.ElapsedMilliseconds);

            var (externalTools, servers) = await DiscoverExternalDescriptorsAsync(cancellationToken);
            item.ExternalTools = externalTools;
            item.Servers = servers;
            _logger.LogDebug("External discovery produced {Count} tools across {Servers} servers in {Total}ms",
                item.ExternalTools.Count, item.Servers.Count, overall.ElapsedMilliseconds);

            await _cache.SetAsync(
                nameof(MCPCatalogCacheItem),
                item,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.Add(CacheAbsoluteLifetime)
                },
                token: cancellationToken);

            _logger.LogInformation(
                "MCP catalog cache rebuilt: {Internal} internal, {External} external, {Servers} servers in {Total}ms",
                item.InternalTools.Count, item.ExternalTools.Count, item.Servers.Count, overall.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild MCP catalog cache");
            throw;
        }
    }

    private async Task<MCPCatalogCacheItem> WaitForCatalogAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < CacheMissPollAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = await _cache.GetAsync(nameof(MCPCatalogCacheItem), token: cancellationToken)
                .ConfigureAwait(false);
            if (item != null)
            {
                return item;
            }

            await Task.Delay(CacheMissPollInterval, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogWarning("MCP catalog cache still empty after polling; returning empty catalog");
        return new MCPCatalogCacheItem { BuiltAtUtc = DateTime.UtcNow };
    }

    private async Task<List<MCPToolDescriptor>> DiscoverInternalDescriptorsAsync(CancellationToken cancellationToken)
    {
        var step = Stopwatch.StartNew();
        var discovered = await _internalToolDiscovery.DiscoverToolsAsync(cancellationToken);
        _logger.LogDebug("Reflection discovery returned {Count} tools in {Elapsed}ms",
            discovered.Count, step.ElapsedMilliseconds);

        var sufiAiTools = _serviceProvider.GetServices<ISufiAITool>().ToList();
        _logger.LogDebug("ISufiAITool DI registrations: {Count}", sufiAiTools.Count);

        var all = discovered
            .Select(ToDescriptor)
            .Concat(sufiAiTools.Select(tool => ToDescriptor(new McpToolAdapter(tool))))
            .ToList();

        EnsureUnique(all, "internal");
        return all;
    }

    private async Task<(List<MCPToolDescriptor> Tools, List<MCPServerSnapshot> Servers)> DiscoverExternalDescriptorsAsync(
        CancellationToken cancellationToken)
    {
        var step = Stopwatch.StartNew();
        List<MCPServer> servers;
        try
        {
            servers = await _serverRepository.GetEnabledListAsync(cancellationToken);
            _logger.LogDebug("Loaded {Count} enabled MCP servers in {Elapsed}ms",
                servers.Count, step.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load enabled MCP servers; external catalog will be empty");
            return (new List<MCPToolDescriptor>(), new List<MCPServerSnapshot>());
        }

        var tools = new List<MCPToolDescriptor>();
        var snapshots = new List<MCPServerSnapshot>();

        foreach (var server in servers)
        {
            snapshots.Add(new MCPServerSnapshot
            {
                Id = server.Id,
                Key = server.Key,
                Name = server.Name,
                TransportType = server.TransportType.ToString(),
                IsEnabled = server.IsEnabled
            });

            var perServer = Stopwatch.StartNew();
            try
            {
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(ExternalDiscoveryTimeout);

                var definitions = await ListServerToolsAsync(server, timeoutSource.Token);
                foreach (var definition in definitions)
                {
                    tools.Add(new MCPToolDescriptor
                    {
                        Name = $"{ExternalPrefix}{server.Key}.{definition.Name}",
                        Description = definition.Description,
                        ParameterSchema = definition.ParameterSchema,
                        ToolType = MCPToolType.External,
                        Source = server.Name
                    });
                }

                _logger.LogDebug("Server {Key} exposed {Count} tools in {Elapsed}ms",
                    server.Key, definitions.Count, perServer.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to discover tools from MCP server {Key} ({Transport}) after {Elapsed}ms",
                    server.Key, server.TransportType, perServer.ElapsedMilliseconds);
            }
        }

        EnsureUnique(tools, "tenant");
        return (tools, snapshots);
    }

    private async Task<List<MCPServerToolDefinition>> ListServerToolsAsync(MCPServer server, CancellationToken cancellationToken)
    {
        IMCPTransportClient? client = null;
        try
        {
            client = CreateClient(server);
            await client.ConnectAsync(cancellationToken);
            return await client.ListToolsAsync(cancellationToken);
        }
        finally
        {
            if (client != null)
            {
                try
                {
                    await client.DisconnectAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error disconnecting MCP server {Key} after discovery", server.Key);
                }

                client.Dispose();
            }
        }
    }

    private IMCPTransportClient CreateClient(MCPServer server)
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

    private static MCPToolDescriptor ToDescriptor(IMCPTool tool) => new()
    {
        Name = tool.Name,
        Description = tool.Description,
        ParameterSchema = tool.ParameterSchema,
        ToolType = tool.ToolType,
        Source = tool.Source
    };

    private static void EnsureUnique(List<MCPToolDescriptor> tools, string scope)
    {
        var duplicate = tools
            .GroupBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate != null)
        {
            throw new BusinessException(AIErrorCodes.MCPDuplicateToolName)
                .WithData("ToolName", duplicate.Key)
                .WithData("Scope", scope);
        }
    }
}
