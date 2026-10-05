using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class EfCoreWorkspaceRepository : EfCoreRepository<IAIDbContext, Workspace, Guid>, IWorkspaceRepository
{
    private readonly IInheritedWorkspaceLiveView _liveView;

    public EfCoreWorkspaceRepository(
        IDbContextProvider<IAIDbContext> dbContextProvider,
        IInheritedWorkspaceLiveView liveView)
        : base(dbContextProvider)
    {
        _liveView = liveView;
    }

    public override async Task<IQueryable<Workspace>> GetQueryableAsync()
    {
        if (CurrentTenant.Id != null || !DataFilter.IsEnabled<IMultiTenant>())
        {
            return await base.GetQueryableAsync();
        }

        // Host scope is TenantId IS NULL. A null CurrentTenant parameter in the
        // global filter is not a reliable IS NULL predicate once filters are ignored
        // or the comparison is parameterized, and it must not return tenant rows.
        var query = (await GetDbSetAsync())
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == null);

        if (DataFilter.IsEnabled<ISoftDelete>())
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!ShouldTrackingEntityChange())
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    public override async Task<IQueryable<Workspace>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(x => x.ModelConfigurations)
            .Include(x => x.Guardrails);
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
        var workspace = await (await WithDetailsAsync())
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
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
        var query = (await WithDetailsAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.DefaultModel.Contains(filter!));

        var workspaces = await query
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
        return await _liveView.ApplyAsync(workspaces, cancellationToken);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        return await query
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.DefaultModel.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }
}
