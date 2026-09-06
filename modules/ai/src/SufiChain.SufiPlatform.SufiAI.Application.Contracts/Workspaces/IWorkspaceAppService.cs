using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public interface IWorkspaceAppService : IApplicationService
{
    Task<PagedResultDto<WorkspaceDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    
    Task<WorkspaceDto> GetAsync(Guid id);

    Task<WorkspaceReadinessDto> GetReadinessAsync(Guid id);
    
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input);

    Task<WorkspaceDto> CloneAsync(Guid id, CloneWorkspaceDto input);
    
    Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input);

    Task<WorkspaceDto> UpdateGuardrailsAsync(Guid id, UpdateWorkspaceGuardrailsDto input);

    Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input);

    Task TestConnectionAsync(TestWorkspaceConnectionInput input);

    Task DeleteAsync(Guid id);
}
