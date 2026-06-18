using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.MCP.Servers;
using Volo.Abp;

namespace SufiChain.SufiAbp.AIManagement.Controllers;

[Area(AIManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai-management/mcp/servers")]
public class MCPServerController : AIManagementController, IMCPServerAppService
{
    private readonly IMCPServerAppService _mcpServerAppService;

    public MCPServerController(IMCPServerAppService mcpServerAppService)
    {
        _mcpServerAppService = mcpServerAppService;
    }

    [HttpGet("by-workspace/{workspaceId}")]
    public virtual Task<List<MCPServerDto>> GetByWorkspaceAsync(Guid workspaceId)
    {
        return _mcpServerAppService.GetByWorkspaceAsync(workspaceId);
    }

    [HttpGet("{id}")]
    public virtual Task<MCPServerDto> GetAsync(Guid id)
    {
        return _mcpServerAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<MCPServerDto> CreateAsync(CreateMCPServerDto input)
    {
        return _mcpServerAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<MCPServerDto> UpdateAsync(Guid id, UpdateMCPServerDto input)
    {
        return _mcpServerAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _mcpServerAppService.DeleteAsync(id);
    }

    [HttpPost("{id}/enable")]
    public virtual Task EnableAsync(Guid id)
    {
        return _mcpServerAppService.EnableAsync(id);
    }

    [HttpPost("{id}/disable")]
    public virtual Task DisableAsync(Guid id)
    {
        return _mcpServerAppService.DisableAsync(id);
    }

    [HttpPost("{id}/test-connection")]
    public virtual Task<bool> TestConnectionAsync(Guid id)
    {
        return _mcpServerAppService.TestConnectionAsync(id);
    }
}
