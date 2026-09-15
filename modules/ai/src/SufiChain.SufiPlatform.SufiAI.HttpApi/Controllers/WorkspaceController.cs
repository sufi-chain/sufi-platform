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

    [HttpGet("lookup")]
    public virtual Task<List<WorkspaceDto>> GetLookupAsync()
    {
        return _workspaceAppService.GetLookupAsync();
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

    [HttpPost("{id}/clone")]
    public virtual Task<WorkspaceDto> CloneAsync(Guid id, CloneWorkspaceDto input)
    {
        return _workspaceAppService.CloneAsync(id, input);
    }

    [HttpPost("{id}/convert-to-custom")]
    public virtual Task<WorkspaceDto> ConvertToCustomAsync(Guid id)
    {
        return _workspaceAppService.ConvertToCustomAsync(id);
    }

    [HttpPost("{id}/assign-to-tenant")]
    public virtual Task<WorkspaceDto> AssignToTenantAsync(Guid id, AssignWorkspaceToTenantDto input)
    {
        return _workspaceAppService.AssignToTenantAsync(id, input);
    }

    [HttpGet("{id}/assignments")]
    public virtual Task<List<WorkspaceAssignmentDto>> GetAssignmentsAsync(Guid id)
    {
        return _workspaceAppService.GetAssignmentsAsync(id);
    }

    [HttpPost("assignments/{assignmentId}/deactivate")]
    public virtual Task DeactivateAssignmentAsync(Guid assignmentId)
    {
        return _workspaceAppService.DeactivateAssignmentAsync(assignmentId);
    }

    [HttpGet("assignable-tenants")]
    public virtual Task<List<AssignableTenantDto>> GetAssignableTenantsAsync()
    {
        return _workspaceAppService.GetAssignableTenantsAsync();
    }

    [HttpPut("{id}/guardrails")]
    public virtual Task<WorkspaceDto> UpdateGuardrailsAsync(Guid id, UpdateWorkspaceGuardrailsDto input)
    {
        return _workspaceAppService.UpdateGuardrailsAsync(id, input);
    }

    [HttpGet("{id}/guardrail-status")]
    public virtual Task<List<WorkspaceGuardrailStatusDto>> GetGuardrailStatusAsync(Guid id)
    {
        return _workspaceAppService.GetGuardrailStatusAsync(id);
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
