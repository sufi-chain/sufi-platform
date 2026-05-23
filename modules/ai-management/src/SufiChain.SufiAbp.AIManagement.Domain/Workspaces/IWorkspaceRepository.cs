using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public interface IWorkspaceRepository : IRepository<Workspace, Guid>
{
    Task<Workspace?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    
    Task<List<Workspace>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "Name",
        CancellationToken cancellationToken = default
    );
    
    Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default);
}
