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

    [HttpGet]
    public virtual Task<List<MCPToolDto>> GetCatalogAsync()
    {
        return _mcpToolAppService.GetCatalogAsync();
    }

    [HttpGet("by-name")]
    public virtual Task<MCPToolDto?> GetAsync([FromQuery] string toolName)
    {
        return _mcpToolAppService.GetAsync(toolName);
    }

    [HttpPost("resolve")]
    public virtual Task<MCPToolResolutionResultDto> ResolveAsync(MCPToolResolutionRequestDto request)
    {
        return _mcpToolAppService.ResolveAsync(request);
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
