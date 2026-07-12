using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceManager : DomainService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    
    public WorkspaceManager(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }
    
    public async Task ValidateNameAsync(string name, Guid? excludeId = null)
    {
        var existing = await _workspaceRepository.FindByNameAsync(name);
        
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNameAlreadyExists)
                .WithData("Name", name);
        }
    }
}
