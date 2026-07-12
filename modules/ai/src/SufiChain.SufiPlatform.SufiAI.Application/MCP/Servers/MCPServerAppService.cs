using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
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
    
    public MCPServerAppService(
        IMCPServerRepository serverRepository,
        IRepository<MCPServer, Guid> repository)
    {
        _serverRepository = serverRepository;
        _repository = repository;
    }
    
    public async Task<List<MCPServerDto>> GetByWorkspaceAsync(Guid workspaceId)
    {
        var servers = await _serverRepository.GetByWorkspaceAsync(workspaceId);
        
        return servers.Select(MapToDto).ToList();
    }
    
    public async Task<MCPServerDto> GetAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        return MapToDto(server);
    }
    
    public async Task<MCPServerDto> CreateAsync(CreateMCPServerDto input)
    {
        // Check for duplicate name in workspace
        var existing = await _serverRepository.FindByNameAsync(input.WorkspaceId, input.Name);
        if (existing != null)
        {
            throw new BusinessException("AI:MCPServerNameAlreadyExists")
                .WithData("Name", input.Name)
                .WithData("WorkspaceId", input.WorkspaceId);
        }
        
        var transportType = Enum.Parse<MCPTransportType>(input.TransportType, ignoreCase: true);
        
        var server = new MCPServer(
            GuidGenerator.Create(),
            input.Name,
            input.WorkspaceId,
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
        
        server.SetMetadata(input.MetadataJson);
        
        await _repository.InsertAsync(server);
        
        return MapToDto(server);
    }
    
    public async Task<MCPServerDto> UpdateAsync(Guid id, UpdateMCPServerDto input)
    {
        var server = await _repository.GetAsync(id);
        
        // Check for duplicate name if name changed
        if (server.Name != input.Name)
        {
            var existing = await _serverRepository.FindByNameAsync(server.WorkspaceId, input.Name);
            if (existing != null && existing.Id != id)
            {
                throw new BusinessException("AI:MCPServerNameAlreadyExists")
                    .WithData("Name", input.Name)
                    .WithData("WorkspaceId", server.WorkspaceId);
            }
        }
        
        server.SetName(input.Name);
        
        if (server.TransportType == MCPTransportType.STDIO)
        {
            server.ConfigureStdio(input.Command!, input.ArgumentsJson);
        }
        else
        {
            server.ConfigureHttpEndpoint(input.Endpoint!);
        }
        
        server.SetMetadata(input.MetadataJson);
        
        await _repository.UpdateAsync(server);
        
        return MapToDto(server);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
    
    public async Task EnableAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        server.Enable();
        await _repository.UpdateAsync(server);
    }
    
    public async Task DisableAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        server.Disable();
        await _repository.UpdateAsync(server);
    }
    
    public async Task<bool> TestConnectionAsync(Guid id)
    {
        var server = await _repository.GetAsync(id);
        
        // TODO: Implement actual connection test
        // For now, just return true if enabled
        return server.IsEnabled;
    }
    
    private MCPServerDto MapToDto(MCPServer server)
    {
        return new MCPServerDto
        {
            Id = server.Id,
            Name = server.Name,
            WorkspaceId = server.WorkspaceId,
            TransportType = server.TransportType.ToString(),
            Endpoint = server.Endpoint,
            Command = server.Command,
            ArgumentsJson = server.ArgumentsJson,
            IsEnabled = server.IsEnabled,
            MetadataJson = server.MetadataJson,
            LastConnectedAt = server.LastConnectedAt,
            LastConnectionError = server.LastConnectionError,
            CreationTime = server.CreationTime,
            CreatorId = server.CreatorId,
            LastModificationTime = server.LastModificationTime,
            LastModifierId = server.LastModifierId
        };
    }
}
