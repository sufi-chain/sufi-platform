using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/workspaces")]
public class WorkspaceController : AIController, IWorkspaceAppService
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

    [HttpGet("{id}/readiness")]
    public virtual Task<WorkspaceReadinessDto> GetReadinessAsync(Guid id)
    {
        return _workspaceAppService.GetReadinessAsync(id);
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

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _workspaceAppService.DeleteAsync(id);
    }
}
