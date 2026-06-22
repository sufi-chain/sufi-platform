using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AI.Workspaces;

public class EfCoreWorkspaceRepository : EfCoreRepository<AIDbContext, Workspace, Guid>, IWorkspaceRepository
{
    public EfCoreWorkspaceRepository(IDbContextProvider<AIDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Workspace?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<List<Workspace>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "Name",
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        
        var query = dbSet
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!));

        return await query
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        
        return await dbSet
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }
}
