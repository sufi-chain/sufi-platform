using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

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

    /// <summary>
    /// Stored inherited rows, including soft-deleted ones when the caller disables that filter.
    /// Does not project the host source. Used to blank copied secrets.
    /// </summary>
    Task<List<Workspace>> ListInheritedStoredAsync(CancellationToken cancellationToken = default);
}
