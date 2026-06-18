using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;

namespace SufiChain.SufiAbp.AIManagement.Controllers;

[Area(AIManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai-management/workspaces")]
public class WorkspaceController : AIManagementController, IWorkspaceAppService
{
    private readonly IWorkspaceAppService _workspaceAppService;

    public WorkspaceController(IWorkspaceAppService workspaceAppService)
    {
        _workspaceAppService = workspaceAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<WorkspaceDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
    {
        return _workspaceAppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<WorkspaceDto> GetAsync(Guid id)
    {
        return _workspaceAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input)
    {
        return _workspaceAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input)
    {
        return _workspaceAppService.UpdateAsync(id, input);
    }

    [HttpPost("available-models")]
    public virtual Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input)
    {
        return _workspaceAppService.GetAvailableModelsAsync(input);
    }

    [HttpPost("test-connection")]
    public virtual Task TestConnectionAsync(TestWorkspaceConnectionInput input)
    {
        return _workspaceAppService.TestConnectionAsync(input);
    }

    [HttpGet("{id}/mcp-tools/configuration")]
    public virtual Task<WorkspaceMCPToolConfigurationDto> GetMCPToolConfigurationAsync(Guid id)
    {
        return _workspaceAppService.GetMCPToolConfigurationAsync(id);
    }

    [HttpPut("{id}/mcp-tools/configuration")]
    public virtual Task UpdateMCPToolConfigurationAsync(Guid id, UpdateWorkspaceMCPToolConfigurationDto input)
    {
        return _workspaceAppService.UpdateMCPToolConfigurationAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _workspaceAppService.DeleteAsync(id);
    }
}
