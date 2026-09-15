using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public interface IInheritedWorkspaceProjectionSynchronizer
{
    Task EnsureCurrentTenantAsync(CancellationToken cancellationToken = default);

    Task<Workspace> CreateTenantProjectionAsync(
        Workspace template,
        Guid tenantId,
        Guid assignmentId,
        Guid targetWorkspaceId,
        Guid sourceWorkspaceId,
        string? preferredName,
        CancellationToken cancellationToken = default);

    Task DeactivateTenantProjectionAsync(
        Guid tenantId,
        Guid targetWorkspaceId,
        CancellationToken cancellationToken = default);

    Task DeactivateHostAssignmentAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default);
}
