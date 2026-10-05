using System.Linq.Dynamic.Core;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.SufiAI.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class MongoWorkspaceRepository : MongoDbRepository<AIMongoDbContext, Workspace, Guid>, IWorkspaceRepository
{
    private readonly IInheritedWorkspaceLiveView _liveView;

    public MongoWorkspaceRepository(
        IMongoDbContextProvider<AIMongoDbContext> dbContextProvider,
        IInheritedWorkspaceLiveView liveView)
        : base(dbContextProvider)
    {
        _liveView = liveView;
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

    public override async Task<Workspace?> FindAsync(
        Guid id,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var workspace = await base.FindAsync(id, includeDetails, cancellationToken);
        if (workspace == null)
        {
            return null;
        }

        return await _liveView.ApplyAsync(workspace, includeDetails, cancellationToken);
    }

    public override async Task<Workspace> UpdateAsync(
        Workspace entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        if (!InheritedWorkspaceLiveView.IsLiveView(entity))
        {
            return await base.UpdateAsync(entity, autoSave, cancellationToken);
        }

        var persisted = await base.FindAsync(entity.Id, includeDetails: true, cancellationToken);
        if (persisted == null)
        {
            throw new EntityNotFoundException(typeof(Workspace), entity.Id);
        }

        persisted.CopyInheritedMutationFrom(entity);
        return await base.UpdateAsync(persisted, autoSave, cancellationToken);
    }

    public async Task<Workspace?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var queryable = await GetHostScopedQueryableAsync(cancellationToken);
        var workspace = await queryable.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
        if (workspace == null)
        {
            return null;
        }

        return await _liveView.ApplyAsync(workspace, includeDetails: true, cancellationToken);
    }

    public async Task<List<Workspace>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "Name",
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetHostScopedQueryableAsync(cancellationToken);
        
        var workspaces = await queryable
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!))
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
        return await _liveView.ApplyAsync(workspaces, cancellationToken);
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
