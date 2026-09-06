using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class MongoWorkspaceAssignmentRepository
    : MongoDbRepository<AIMongoDbContext, WorkspaceAssignment, Guid>, IWorkspaceAssignmentRepository
{
    public MongoWorkspaceAssignmentRepository(IMongoDbContextProvider<AIMongoDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    public async Task<List<WorkspaceAssignment>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var queryable = await GetMongoQueryableAsync(cancellationToken);
        return await queryable.Where(x => x.TenantId == tenantId && x.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceAssignment?> FindAsync(Guid tenantId, Guid sourceWorkspaceId, CancellationToken cancellationToken = default)
    {
        var queryable = await GetMongoQueryableAsync(cancellationToken);
        return await queryable.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SourceWorkspaceId == sourceWorkspaceId, cancellationToken);
    }
}
