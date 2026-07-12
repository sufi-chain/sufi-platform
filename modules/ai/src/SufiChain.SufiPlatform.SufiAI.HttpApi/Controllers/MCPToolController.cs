using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/mcp/tools")]
public class MCPToolController : AIController, IMCPToolAppService
{
    private readonly IMCPToolAppService _mcpToolAppService;

    public MCPToolController(IMCPToolAppService mcpToolAppService)
    {
        _mcpToolAppService = mcpToolAppService;
    }

    [HttpGet("by-workspace/{workspaceName}")]
    public virtual Task<List<MCPToolDto>> GetToolsForWorkspaceAsync(string workspaceName)
    {
        return _mcpToolAppService.GetToolsForWorkspaceAsync(workspaceName);
    }

    [HttpGet("by-workspace/{workspaceName}/{toolName}")]
    public virtual Task<MCPToolDto> GetToolAsync(string workspaceName, string toolName)
    {
        return _mcpToolAppService.GetToolAsync(workspaceName, toolName);
    }

    [HttpPost("execute")]
    public virtual Task<MCPToolExecutionResultDto> ExecuteToolAsync(MCPToolExecutionRequestDto request)
    {
        return _mcpToolAppService.ExecuteToolAsync(request);
    }

    [HttpPost("refresh-registry")]
    public virtual Task RefreshToolRegistryAsync()
    {
        return _mcpToolAppService.RefreshToolRegistryAsync();
    }
}
