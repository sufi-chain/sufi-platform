using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.Domain.Repositories;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.SufiAI.Application.MCP.Servers;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.MCPServers.Default)]
public class MCPServerAppService : SufiApplicationService, IMCPServerAppService
{
    private readonly IMCPServerRepository _serverRepository;
    private readonly IRepository<MCPServer, Guid> _repository;
    private readonly IMCPToolRegistry _toolRegistry;
    private readonly IMCPCatalogCache _catalogCache;
    
    public MCPServerAppService(
        IMCPServerRepository serverRepository,
        IRepository<MCPServer, Guid> repository,
        IMCPToolRegistry toolRegistry,
        IMCPCatalogCache catalogCache)
    {
        _serverRepository = serverRepository;
        _repository = repository;
        _toolRegistry = toolRegistry;
        _catalogCache = catalogCache;
    }
    
    public async Task<List<MCPServerDto>> GetListAsync()
    {
        var servers = await _serverRepository.GetListAsync();
        
        return servers.Select(MapToDto).ToList();
    }
    
    public async Task<MCPServerDto> GetAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        return MapToDto(server);
    }
    
    public async Task<MCPServerDto> CreateAsync(CreateMCPServerDto input)
    {
        var existing = await _serverRepository.FindByKeyAsync(input.Key);
        if (existing != null)
        {
            throw new BusinessException("AI:MCPServerKeyAlreadyExists")
                .WithData("Key", input.Key);
        }
        
        var transportType = Enum.Parse<MCPTransportType>(input.TransportType, ignoreCase: true);
        
        var server = new MCPServer(
            GuidGenerator.Create(),
            input.Key,
            input.Name,
            transportType,
            CurrentTenant.Id
        );
        
        if (transportType == MCPTransportType.STDIO)
        {
            server.ConfigureStdio(input.Command!, input.ArgumentsJson);
        }
        else
        {
            server.ConfigureHttpEndpoint(input.Endpoint!);
        }
        
        await _repository.InsertAsync(server);
        await _catalogCache.InvalidateAsync();
        
        return MapToDto(server);
    }
    
    public async Task<MCPServerDto> UpdateAsync(Guid id, UpdateMCPServerDto input)
    {
        var server = await _repository.GetAsync(id);
        
        server.SetName(input.Name);
        
        if (server.TransportType == MCPTransportType.STDIO)
        {
            server.ConfigureStdio(input.Command!, input.ArgumentsJson);
        }
        else
        {
            server.ConfigureHttpEndpoint(input.Endpoint!);
        }
        
        await _repository.UpdateAsync(server);
        await _catalogCache.InvalidateAsync();
        
        return MapToDto(server);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        await _catalogCache.InvalidateAsync();
    }
    
    public async Task EnableAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        server.Enable();
        await _repository.UpdateAsync(server);
        await _catalogCache.InvalidateAsync();
    }
    
    public async Task DisableAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        server.Disable();
        await _repository.UpdateAsync(server);
        await _catalogCache.InvalidateAsync();
    }
    
    public async Task<bool> TestConnectionAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);

        // Disabled servers cannot connect.
        if (!server.IsEnabled)
        {
            server.UpdateLastConnection(false, "Server is disabled");
            await _repository.UpdateAsync(server);
            return false;
        }

        // HTTP transport is not implemented; STDIO/SSE require command or endpoint.
        if (server.TransportType == MCPTransportType.HTTP)
        {
            server.UpdateLastConnection(false, "HTTP transport is not supported");
            await _repository.UpdateAsync(server);
            return false;
        }

        if (server.TransportType == MCPTransportType.STDIO && string.IsNullOrWhiteSpace(server.Command))
        {
            server.UpdateLastConnection(false, "STDIO server is missing command");
            await _repository.UpdateAsync(server);
            return false;
        }

        if (server.TransportType == MCPTransportType.SSE && string.IsNullOrWhiteSpace(server.Endpoint))
        {
            server.UpdateLastConnection(false, "SSE server is missing endpoint");
            await _repository.UpdateAsync(server);
            return false;
        }

        // Lightweight connect via registry transport clients (not cached for test).
        var (success, errorMessage) = await _toolRegistry.TestServerConnectionAsync(server);
        server.UpdateLastConnection(success, errorMessage);
        await _repository.UpdateAsync(server);
        return success;
    }
    
    private MCPServerDto MapToDto(MCPServer server)
    {
        return new MCPServerDto
        {
            Id = server.Id,
            Key = server.Key,
            Name = server.Name,
            TransportType = server.TransportType.ToString(),
            Endpoint = server.Endpoint,
            Command = server.Command,
            ArgumentsJson = server.ArgumentsJson,
            IsEnabled = server.IsEnabled,
            LastConnectedAt = server.LastConnectedAt,
            LastConnectionError = server.LastConnectionError,
            CreationTime = server.CreationTime,
            CreatorId = server.CreatorId,
            LastModificationTime = server.LastModificationTime,
            LastModifierId = server.LastModifierId
        };
    }
}
