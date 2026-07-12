using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Servers;

public interface IMCPServerAppService : IApplicationService
{
    Task<List<MCPServerDto>> GetByWorkspaceAsync(Guid workspaceId);
    
    Task<MCPServerDto> GetAsync(Guid id);
    
    Task<MCPServerDto> CreateAsync(CreateMCPServerDto input);
    
    Task<MCPServerDto> UpdateAsync(Guid id, UpdateMCPServerDto input);
    
    Task DeleteAsync(Guid id);
    
    Task EnableAsync(Guid id);
    
    Task DisableAsync(Guid id);
    
    Task<bool> TestConnectionAsync(Guid id);
}
