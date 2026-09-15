using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>Host control-plane assignment of a workspace to a tenant.</summary>
public class WorkspaceAssignment : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid SourceWorkspaceId { get; protected set; }
    public Guid TargetWorkspaceId { get; protected set; }
    public int Version { get; protected set; }
    public bool IsActive { get; protected set; }

    protected WorkspaceAssignment() { }

    public WorkspaceAssignment(Guid id, Guid tenantId, Guid sourceWorkspaceId, Guid targetWorkspaceId)
        : base(id)
    {
        TenantId = tenantId;
        SourceWorkspaceId = sourceWorkspaceId;
        TargetWorkspaceId = targetWorkspaceId;
        Version = 1;
        IsActive = true;
    }

    public void ReplaceTarget(Guid targetWorkspaceId)
    {
        TargetWorkspaceId = targetWorkspaceId;
        Version++;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
