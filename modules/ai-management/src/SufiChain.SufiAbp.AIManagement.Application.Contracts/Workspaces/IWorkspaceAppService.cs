using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public interface IWorkspaceAppService : IApplicationService
{
    Task<PagedResultDto<WorkspaceDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    
    Task<WorkspaceDto> GetAsync(Guid id);
    
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input);
    
    Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input);

    Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input);

    Task TestConnectionAsync(TestWorkspaceConnectionInput input);

    Task<WorkspaceMCPToolConfigurationDto> GetMCPToolConfigurationAsync(Guid id);

    Task UpdateMCPToolConfigurationAsync(Guid id, UpdateWorkspaceMCPToolConfigurationDto input);
    
    Task DeleteAsync(Guid id);
}
