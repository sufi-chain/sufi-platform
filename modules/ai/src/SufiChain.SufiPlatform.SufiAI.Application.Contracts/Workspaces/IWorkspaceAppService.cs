using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public interface IWorkspaceAppService : IApplicationService
{
    Task<PagedResultDto<WorkspaceDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<List<WorkspaceDto>> GetLookupAsync();
    
    Task<WorkspaceDto> GetAsync(Guid id);

    Task<WorkspaceReadinessDto> GetReadinessAsync(Guid id);
    
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input);

    Task<WorkspaceDto> CloneAsync(Guid id, CloneWorkspaceDto input);

    Task<WorkspaceDto> ConvertToCustomAsync(Guid id);

    Task<WorkspaceDto> AssignToTenantAsync(Guid id, AssignWorkspaceToTenantDto input);

    Task<List<WorkspaceAssignmentDto>> GetAssignmentsAsync(Guid id);

    Task DeactivateAssignmentAsync(Guid assignmentId);

    Task<List<AssignableTenantDto>> GetAssignableTenantsAsync();
    
    Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input);

    Task<WorkspaceDto> UpdateGuardrailsAsync(Guid id, UpdateWorkspaceGuardrailsDto input);

    Task<List<WorkspaceGuardrailStatusDto>> GetGuardrailStatusAsync(Guid id);

    Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input);

    Task TestConnectionAsync(TestWorkspaceConnectionInput input);

    Task DeleteAsync(Guid id);
}
