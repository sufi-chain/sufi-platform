using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Adapters;
using SufiChain.SufiAbp.AI.MCP.Abstractions;
using SufiChain.SufiAbp.AI.MCP.Entities;
using SufiChain.SufiAbp.AI.MCP.External;
using SufiChain.SufiAbp.AI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.MCP.Registry;

/// <summary>
/// Central registry for all MCP tools (internal and external).
/// </summary>
public class MCPToolRegistry : IMCPToolRegistry, ISingletonDependency
{
    private readonly IInternalToolDiscoveryService _internalToolDiscovery;
    private readonly IMCPServerRepository _serverRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MCPToolRegistry> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Guid, IMCPTransportClient> _activeClients = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    public MCPToolRegistry(
        IInternalToolDiscoveryService internalToolDiscovery,
        IMCPServerRepository serverRepository,
        IWorkspaceRepository workspaceRepository,
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider,
        ILogger<MCPToolRegistry> logger)
    {
        _internalToolDiscovery = internalToolDiscovery;
        _serverRepository = serverRepository;
        _workspaceRepository = workspaceRepository;
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task<List<IMCPTool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        var enabledToolNames = ReadEnabledMCPToolNames(workspace);
        if (enabledToolNames.Count == 0)
        {
            return new List<IMCPTool>();
        }

        var allTools = await GetAllToolsForWorkspaceAsync(workspaceName, cancellationToken);
        var enabledToolNameSet = enabledToolNames.ToHashSet();
        return allTools.Where(tool => enabledToolNameSet.Contains(tool.Name)).ToList();
    }

    public async Task<List<IMCPTool>> GetAllToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }
        
        var tools = new List<IMCPTool>();
        
        // Get internal tools
        var internalTools = await _internalToolDiscovery.DiscoverToolsAsync(cancellationToken);
        tools.AddRange(internalTools);

        var frameworkTools = _serviceProvider
            .GetServices<ISufiAITool>()
            .Where(tool => tools.All(existingTool => existingTool.Name != tool.Name))
            .Select(tool => (IMCPTool)new McpToolAdapter(tool));
        tools.AddRange(frameworkTools);
        
        // Get external tools from MCP servers
        var servers = await _serverRepository.GetEnabledByWorkspaceAsync(workspace.Id, cancellationToken);
        
        foreach (var server in servers)
        {
            try
            {
                var client = await GetOrCreateClientAsync(server, cancellationToken);
                var serverTools = await client.ListToolsAsync(cancellationToken);
                
                foreach (var toolDef in serverTools)
                {
                    var tool = new ExternalMCPTool(
                        toolDef.Name,
                        toolDef.Description,
                        toolDef.ParameterSchema,
                        server.Id,
                        server.Name,
                        client
                    );
                    
                    tools.Add(tool);
                }
                
                server.UpdateLastConnection(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to load tools from MCP server {ServerName} (ID: {ServerId})",
                    server.Name,
                    server.Id
                );
                
                server.UpdateLastConnection(false, ex.Message);
            }
        }
        
        _logger.LogInformation(
            "Loaded {ToolCount} tools for workspace {WorkspaceName} ({InternalCount} internal, {ExternalCount} external)",
            tools.Count,
            workspaceName,
            internalTools.Count,
            tools.Count - internalTools.Count
        );
        
        return tools;
    }

    private static List<string> ReadEnabledMCPToolNames(Workspace workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace.EnabledMCPToolsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(workspace.EnabledMCPToolsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
    
    public async Task<IMCPTool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default)
    {
        var tools = await GetToolsForWorkspaceAsync(workspaceName, cancellationToken);
        return tools.FirstOrDefault(t => t.Name == toolName);
    }
    
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Refresh internal tools
            await _internalToolDiscovery.RefreshAsync(cancellationToken);
            
            // Disconnect all external clients (they'll reconnect on next use)
            foreach (var client in _activeClients.Values)
            {
                try
                {
                    await client.DisconnectAsync(cancellationToken);
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disconnecting MCP client during refresh");
                }
            }
            
            _activeClients.Clear();
            
            _logger.LogInformation("MCP tool registry refreshed");
        }
        finally
        {
            _lock.Release();
        }
    }
    
    private async Task<IMCPTransportClient> GetOrCreateClientAsync(
        MCPServer server,
        CancellationToken cancellationToken)
    {
        if (_activeClients.TryGetValue(server.Id, out var existingClient) && existingClient.IsConnected)
        {
            return existingClient;
        }
        
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_activeClients.TryGetValue(server.Id, out existingClient) && existingClient.IsConnected)
            {
                return existingClient;
            }
            
            IMCPTransportClient client = server.TransportType switch
            {
                MCPTransportType.STDIO => CreateStdioClient(server),
                MCPTransportType.SSE => CreateSSEClient(server),
                MCPTransportType.HTTP => throw new NotImplementedException("HTTP transport not yet implemented"),
                _ => throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                    .WithData("TransportType", server.TransportType)
            };
            
            await client.ConnectAsync(cancellationToken);
            
            _activeClients[server.Id] = client;
            
            return client;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    private IMCPTransportClient CreateStdioClient(MCPServer server)
    {
        if (string.IsNullOrEmpty(server.Command))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("Reason", "STDIO server missing command");
        }
        
        var arguments = string.IsNullOrEmpty(server.ArgumentsJson)
            ? Array.Empty<string>()
            : JsonSerializer.Deserialize<string[]>(server.ArgumentsJson) ?? Array.Empty<string>();
        
        var logger = _logger as ILogger<StdioTransportClient> 
            ?? throw new InvalidOperationException("Logger type mismatch");
        
        return new StdioTransportClient(server.Command, arguments, logger);
    }
    
    private IMCPTransportClient CreateSSEClient(MCPServer server)
    {
        if (string.IsNullOrEmpty(server.Endpoint))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("Reason", "SSE server missing endpoint");
        }
        
        var httpClient = _httpClientFactory.CreateClient();
        
        var logger = _logger as ILogger<SSETransportClient> 
            ?? throw new InvalidOperationException("Logger type mismatch");
        
        return new SSETransportClient(server.Endpoint, httpClient, logger);
    }
}
