using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class EfCoreWorkspaceAssignmentRepository
    : EfCoreRepository<IAIDbContext, WorkspaceAssignment, Guid>, IWorkspaceAssignmentRepository
{
    public EfCoreWorkspaceAssignmentRepository(IDbContextProvider<IAIDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<WorkspaceAssignment>> GetActiveByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceAssignment?> FindAsync(
        Guid tenantId,
        Guid sourceWorkspaceId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.SourceWorkspaceId == sourceWorkspaceId,
                cancellationToken);
    }

    public async Task<List<WorkspaceAssignment>> GetListBySourceWorkspaceAsync(
        Guid sourceWorkspaceId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SourceWorkspaceId == sourceWorkspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceAssignment?> FindByTargetWorkspaceAsync(
        Guid targetWorkspaceId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.TargetWorkspaceId == targetWorkspaceId, cancellationToken);
    }
}
