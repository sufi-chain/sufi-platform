using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public interface IWorkspaceAssignmentRepository : IRepository<WorkspaceAssignment, Guid>
{
    Task<List<WorkspaceAssignment>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<WorkspaceAssignment?> FindAsync(Guid tenantId, Guid sourceWorkspaceId, CancellationToken cancellationToken = default);
    Task<List<WorkspaceAssignment>> GetListBySourceWorkspaceAsync(Guid sourceWorkspaceId, CancellationToken cancellationToken = default);
    Task<WorkspaceAssignment?> FindByTargetWorkspaceAsync(Guid targetWorkspaceId, CancellationToken cancellationToken = default);
}
