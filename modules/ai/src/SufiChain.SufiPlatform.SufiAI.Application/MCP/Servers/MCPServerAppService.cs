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
using SufiChain.SufiPlatform.SufiAI.MCP.External;
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
    private readonly IMCPTransportClientProvider _clientProvider;
    
    public MCPServerAppService(
        IMCPServerRepository serverRepository,
        IRepository<MCPServer, Guid> repository,
        IMCPToolRegistry toolRegistry,
        IMCPCatalogCache catalogCache,
        IMCPTransportClientProvider clientProvider)
    {
        _serverRepository = serverRepository;
        _repository = repository;
        _toolRegistry = toolRegistry;
        _catalogCache = catalogCache;
        _clientProvider = clientProvider;
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
        
        var transportType = ParseTransport(input.TransportType);
        
        var server = new MCPServer(
            GuidGenerator.Create(),
            input.Key,
            input.Name,
            transportType,
            CurrentTenant.Id
        );

        // #384 slice 6b: the binding mode is validated by the domain (UserScoped + STDIO is refused).
        server.SetBindingMode(ParseBindingMode(input.BindingMode));

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
        var before = ConnectionSnapshot.From(server);

        server.SetName(input.Name);

        // A null transport or binding keeps the stored value, so an older caller cannot silently
        // downgrade a user-scoped server or switch its transport.
        var transport = string.IsNullOrWhiteSpace(input.TransportType)
            ? server.TransportType
            : ParseTransport(input.TransportType);
        var binding = string.IsNullOrWhiteSpace(input.BindingMode)
            ? server.BindingMode
            : ParseBindingMode(input.BindingMode);

        server.ApplyConfiguration(transport, binding, input.Endpoint, input.Command, input.ArgumentsJson);

        await _repository.UpdateAsync(server);
        await _catalogCache.InvalidateAsync();
        if (before.ConnectionChanged(server))
        {
            await _clientProvider.EvictAsync(server.Id);
        }

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

        if (server.TransportType == MCPTransportType.STDIO && string.IsNullOrWhiteSpace(server.Command))
        {
            server.UpdateLastConnection(false, "STDIO server is missing command");
            await _repository.UpdateAsync(server);
            return false;
        }

        if (server.TransportType != MCPTransportType.STDIO && string.IsNullOrWhiteSpace(server.Endpoint))
        {
            server.UpdateLastConnection(false, $"{server.TransportType} server is missing endpoint");
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
            BindingMode = server.BindingMode.ToString(),
            LastConnectedAt = server.LastConnectedAt,
            LastConnectionError = server.LastConnectionError,
            CreationTime = server.CreationTime,
            CreatorId = server.CreatorId,
            LastModificationTime = server.LastModificationTime,
            LastModifierId = server.LastModifierId
        };
    }

    private static MCPTransportType ParseTransport(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse<MCPTransportType>(value, ignoreCase: true, out var transport)
            || !Enum.IsDefined(transport))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("TransportType", value ?? string.Empty);
        }

        return transport;
    }

    /// <summary>
    /// #384 slice 6b: parses the incoming binding mode. An unknown value is refused instead of silently
    /// falling back to TenantLevel, so a typo cannot downgrade a user-scoped server.
    /// </summary>
    private static MCPBindingMode ParseBindingMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return MCPBindingMode.TenantLevel;
        }

        if (!Enum.TryParse<MCPBindingMode>(value, ignoreCase: true, out var mode)
            || !Enum.IsDefined(mode))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("BindingMode", value);
        }

        return mode;
    }

    private readonly record struct ConnectionSnapshot(
        MCPTransportType TransportType,
        MCPBindingMode BindingMode,
        string? Endpoint,
        string? Command,
        string? ArgumentsJson)
    {
        public static ConnectionSnapshot From(MCPServer server) => new(
            server.TransportType,
            server.BindingMode,
            server.Endpoint,
            server.Command,
            server.ArgumentsJson);

        public bool ConnectionChanged(MCPServer server) =>
            TransportType != server.TransportType
            || BindingMode != server.BindingMode
            || !string.Equals(Endpoint, server.Endpoint, StringComparison.Ordinal)
            || !string.Equals(Command, server.Command, StringComparison.Ordinal)
            || !string.Equals(ArgumentsJson, server.ArgumentsJson, StringComparison.Ordinal);
    }
}
