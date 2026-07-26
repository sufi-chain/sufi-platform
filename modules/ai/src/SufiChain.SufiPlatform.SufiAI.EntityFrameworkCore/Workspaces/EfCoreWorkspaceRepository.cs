using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class EfCoreWorkspaceRepository : EfCoreRepository<IAIDbContext, Workspace, Guid>, IWorkspaceRepository
{
    public EfCoreWorkspaceRepository(IDbContextProvider<IAIDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<Workspace>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).Include(x => x.ModelConfigurations);
    }

    public async Task<Workspace?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await (await WithDetailsAsync())
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<List<Workspace>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "Name",
        CancellationToken cancellationToken = default)
    {
        var query = (await WithDetailsAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.DefaultModel.Contains(filter!));

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
                x => x.Name.Contains(filter!) || x.DefaultModel.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }
}
