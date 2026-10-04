using System.Linq.Dynamic.Core;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.SufiAI.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class MongoWorkspaceRepository : MongoDbRepository<AIMongoDbContext, Workspace, Guid>, IWorkspaceRepository
{
    public MongoWorkspaceRepository(IMongoDbContextProvider<AIMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override Task<IQueryable<Workspace>> GetQueryableAsync()
    {
        return GetHostScopedQueryableAsync(CancellationToken.None);
    }

    public override async Task<IQueryable<Workspace>> GetQueryableAsync(
        CancellationToken cancellationToken = default,
        AggregateOptions? options = null)
    {
        return await GetHostScopedQueryableAsync(cancellationToken, options);
    }

    public async Task<Workspace?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var queryable = await GetHostScopedQueryableAsync(cancellationToken);
        return await queryable.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<List<Workspace>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "Name",
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetHostScopedQueryableAsync(cancellationToken);
        
        return await queryable
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!))
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var queryable = await GetHostScopedQueryableAsync(cancellationToken);
        
        return await queryable
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }

    protected virtual async Task<IQueryable<Workspace>> GetHostScopedQueryableAsync(
        CancellationToken cancellationToken,
        AggregateOptions? options = null)
    {
        var queryable = await base.GetQueryableAsync(cancellationToken, options);
        if (CurrentTenant.Id != null || !DataFilter.IsEnabled<IMultiTenant>())
        {
            return queryable;
        }

        queryable = queryable.Where(x => x.TenantId == null);
        if (DataFilter.IsEnabled<ISoftDelete>())
        {
            queryable = queryable.Where(x => !x.IsDeleted);
        }

        return queryable;
    }
}
