using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Adapters;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Audit;
using SufiChain.SufiPlatform.SufiAI.MCP.Auth;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.External;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Registry;

/// <summary>
/// Tenant-aware registry. Catalog reads (listing pages, tool pickers) are served from the
/// long-lived <see cref="IMCPCatalogCache"/> and never perform live transport I/O. Live
/// transport is only used by <see cref="ResolveAsync"/> (for execution) and
/// <see cref="TestServerConnectionAsync"/>.
/// </summary>
public class MCPToolRegistry : IMCPToolRegistry, ISingletonDependency
{
    private const string ExternalPrefix = "external.";
    private static readonly TimeSpan ExternalDiscoveryTimeout = TimeSpan.FromSeconds(15);

    private readonly IMCPCatalogCache _catalogCache;
    private readonly IInternalToolDiscoveryService _internalToolDiscovery;
    private readonly IMCPServerRepository _serverRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MCPToolRegistry> _logger;
    private readonly IMcpToolCallAuditor? _auditor;

    /// <summary>
    /// sufi-chain#384 slice 6b: owns the transport clients, so a <c>UserScoped</c> server is never handed a
    /// client that was dialled for another user. Resolved lazily from the root provider because the
    /// registry itself is a singleton.
    /// </summary>
    private readonly IMCPTransportClientProvider? _clientProvider;

    private IMCPTransportClientProvider ClientProvider =>
        _clientProvider ?? _serviceProvider.GetRequiredService<IMCPTransportClientProvider>();

    public MCPToolRegistry(
        IMCPCatalogCache catalogCache,
        IInternalToolDiscoveryService internalToolDiscovery,
        IMCPServerRepository serverRepository,
        IServiceProvider serviceProvider,
        ILogger<MCPToolRegistry> logger,
        IMcpToolCallAuditor? auditor = null,
        IMCPTransportClientProvider? clientProvider = null)
    {
        _catalogCache = catalogCache;
        _internalToolDiscovery = internalToolDiscovery;
        _serverRepository = serverRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _auditor = auditor;
        _clientProvider = clientProvider;
    }

    public async Task<List<IMCPTool>> GetInternalToolsAsync(CancellationToken cancellationToken = default)
    {
        var overall = Stopwatch.StartNew();
        var catalog = await _catalogCache.GetCatalogAsync(cancellationToken);
        var tools = catalog.InternalTools
            .Select(descriptor => (IMCPTool)new CachedMCPTool(descriptor))
            .ToList();
        _logger.LogDebug("GetInternalToolsAsync returned {Count} cached tools in {Elapsed}ms",
            tools.Count, overall.ElapsedMilliseconds);
        return tools;
    }

    public async Task<List<IMCPTool>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var overall = Stopwatch.StartNew();
        var catalog = await _catalogCache.GetCatalogAsync(cancellationToken);
        var tools = catalog.InternalTools
            .Concat(catalog.ExternalTools)
            .Select(descriptor => (IMCPTool)new CachedMCPTool(descriptor))
            .ToList();
        _logger.LogDebug(
            "GetCatalogAsync returned {Internal} internal + {External} external = {Total} cached tools in {Elapsed}ms",
            catalog.InternalTools.Count, catalog.ExternalTools.Count, tools.Count, overall.ElapsedMilliseconds);
        return tools;
    }

    public async Task<MCPToolResolutionResult> ResolveAsync(
        IReadOnlyCollection<string> toolNames,
        CancellationToken cancellationToken = default)
    {
        var overall = Stopwatch.StartNew();
        _logger.LogDebug("ResolveAsync invoked for {Count} tool names", toolNames.Count);

        var result = new MCPToolResolutionResult();
        if (toolNames.Count == 0)
        {
            return result;
        }

        var requestedNames = toolNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        _logger.LogDebug("ResolveAsync normalized {Count} distinct requested names", requestedNames.Count);

        // Catalog APIs intentionally return CachedMCPTool stubs. Resolve must return live
        // InternalMCPTool / ISufiAITool adapters so MCPKernelToolRegistrar can execute them.
        var internalTools = (await LoadExecutableInternalToolsAsync(cancellationToken))
            .ToDictionary(tool => tool.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in requestedNames.Where(name => !name.StartsWith(ExternalPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            if (internalTools.TryGetValue(name, out var tool))
            {
                result.Tools.Add(tool);
                _logger.LogDebug("Resolved internal tool {Name}", name);
            }
            else
            {
                result.Diagnostics.Add(Diagnostic(name, "Unknown", "The internal MCP tool is not registered."));
                _logger.LogWarning("ResolveAsync could not find internal tool {Name}", name);
            }
        }

        foreach (var group in requestedNames
                     .Where(name => name.StartsWith(ExternalPrefix, StringComparison.OrdinalIgnoreCase))
                     .GroupBy(ParseServerKey, StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(group.Key))
            {
                foreach (var name in group)
                {
                    result.Diagnostics.Add(Diagnostic(name, "Malformed", "External tool names must use external.<serverKey>.<toolName>."));
                }
                continue;
            }

            var server = await _serverRepository.FindByKeyAsync(group.Key, cancellationToken);
            if (server == null)
            {
                foreach (var name in group)
                {
                    result.Diagnostics.Add(Diagnostic(name, "UnknownServer", $"MCP server key '{group.Key}' is not visible to the current tenant."));
                }
                _logger.LogWarning("ResolveAsync: MCP server key '{Key}' not visible to current tenant", group.Key);
                continue;
            }

            if (!server.IsEnabled)
            {
                foreach (var name in group)
                {
                    result.Diagnostics.Add(Diagnostic(name, "Disabled", $"MCP server '{group.Key}' is disabled."));
                }
                _logger.LogWarning("ResolveAsync: MCP server '{Key}' is disabled", group.Key);
                continue;
            }

            var serverStep = Stopwatch.StartNew();
            try
            {
                var serverTools = (await LoadExternalToolsAsync(server, cancellationToken))
                    .ToDictionary(tool => tool.Name, StringComparer.OrdinalIgnoreCase);
                foreach (var name in group)
                {
                    if (serverTools.TryGetValue(name, out var tool))
                    {
                        result.Tools.Add(tool);
                        _logger.LogDebug("Resolved external tool {Name} from server {Key}", name, group.Key);
                    }
                    else
                    {
                        result.Diagnostics.Add(Diagnostic(name, "UnknownTool", $"MCP server '{group.Key}' does not expose the requested tool."));
                        _logger.LogWarning("MCP server '{Key}' does not expose requested tool {Name}", group.Key, name);
                    }
                }
                server.UpdateLastConnection(true);
                await _serverRepository.UpdateAsync(server, autoSave: true);
                _logger.LogDebug("External server {Key} resolved in {Elapsed}ms", group.Key, serverStep.ElapsedMilliseconds);
            }
            catch (Exception exception)
            {
                var cause = McpConnectionError.Describe(exception);
                server.UpdateLastConnection(false, cause);
                await _serverRepository.UpdateAsync(server, autoSave: true);
                foreach (var name in group)
                {
                    result.Diagnostics.Add(Diagnostic(name, "ConnectionFailed", cause));
                }
                _logger.LogWarning(exception, "ResolveAsync failed to connect MCP server {Key} after {Elapsed}ms", group.Key, serverStep.ElapsedMilliseconds);
            }
        }

        result.Tools = EnsureUnique(result.Tools, "resolved");
        _logger.LogDebug("ResolveAsync completed: {Resolved} tools, {Diagnostics} diagnostics in {Elapsed}ms",
            result.Tools.Count, result.Diagnostics.Count, overall.ElapsedMilliseconds);
        return result;
    }

    private async Task<List<IMCPTool>> LoadExecutableInternalToolsAsync(CancellationToken cancellationToken)
    {
        var discovered = await _internalToolDiscovery.DiscoverToolsAsync(cancellationToken);
        var fromDi = _serviceProvider.GetServices<ISufiAITool>()
            .Select(tool => (IMCPTool)new McpToolAdapter(tool))
            .ToList();

        // Prefer reflection-discovered InternalMCPTool when both expose the same name
        // (Calendar tools are ISufiAITool + [SufiAiMcpTool]; catalog may only keep one).
        var byName = new Dictionary<string, IMCPTool>(StringComparer.OrdinalIgnoreCase);
        foreach (var tool in fromDi.Concat(discovered))
        {
            byName[tool.Name] = tool;
        }

        return byName.Values.ToList();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP registry refresh requested");
        await _internalToolDiscovery.RefreshAsync(cancellationToken);
        await DisconnectAllClientsAsync(cancellationToken);
        await _catalogCache.RebuildAsync(cancellationToken);
        _logger.LogInformation("MCP registry refresh completed");
    }

    public async Task<(bool Success, string? ErrorMessage)> TestServerConnectionAsync(
        MCPServer server,
        CancellationToken cancellationToken = default)
    {
        var step = Stopwatch.StartNew();
        try
        {
            var connected = await ClientProvider.TestConnectionAsync(server, cancellationToken);
            _logger.LogDebug("TestServerConnectionAsync {Key} -> {Result} in {Elapsed}ms",
                server.Key, connected, step.ElapsedMilliseconds);
            return (connected, connected ? null : "Connection failed");
        }
        catch (Exception exception)
        {
            var cause = McpConnectionError.Describe(exception);
            _logger.LogWarning(exception, "TestServerConnectionAsync failed for {Key} after {Elapsed}ms", server.Key, step.ElapsedMilliseconds);
            return (false, cause);
        }
    }

    private Task DisconnectAllClientsAsync(CancellationToken cancellationToken)
    {
        // sufi-chain#384 slice 6b: the provider owns the clients, including the per-call user-scoped ones.
        return ClientProvider.DisconnectAllAsync(cancellationToken);
    }

    private async Task<List<IMCPTool>> LoadExternalToolsAsync(MCPServer server, CancellationToken cancellationToken)
    {
        var step = Stopwatch.StartNew();
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(ExternalDiscoveryTimeout);

        // Discovery is identity-free: a UserScoped server is listed without a token and is only dialled
        // with a user's token when a tool actually executes.
        var definitions = await ClientProvider.ListToolsAsync(server, timeoutSource.Token);
        var tools = definitions.Select(definition => (IMCPTool)new ExternalMCPTool(
            $"{ExternalPrefix}{server.Key}.{definition.Name}",
            definition.Description,
            definition.ParameterSchema,
            server,
            definition.Name,
            ClientProvider,
            _auditor)).ToList();
        _logger.LogDebug("LoadExternalToolsAsync {Key} returned {Count} tools in {Elapsed}ms",
            server.Key, tools.Count, step.ElapsedMilliseconds);
        return tools;
    }

    private static List<IMCPTool> EnsureUnique(IEnumerable<IMCPTool> tools, string scope)
    {
        var list = tools.ToList();
        var duplicate = list
            .GroupBy(tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate != null)
        {
            throw new BusinessException(AIErrorCodes.MCPDuplicateToolName)
                .WithData("ToolName", duplicate.Key)
                .WithData("Scope", scope);
        }
        return list;
    }

    private static string ParseServerKey(string qualifiedName)
    {
        if (!qualifiedName.StartsWith(ExternalPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var remainder = qualifiedName[ExternalPrefix.Length..];
        var separatorIndex = remainder.IndexOf('.');
        return separatorIndex > 0 && separatorIndex < remainder.Length - 1
            ? remainder[..separatorIndex]
            : string.Empty;
    }

    private static MCPToolResolutionDiagnostic Diagnostic(string name, string code, string message)
    {
        return new MCPToolResolutionDiagnostic { ToolName = name, Code = code, Message = message };
    }
}
