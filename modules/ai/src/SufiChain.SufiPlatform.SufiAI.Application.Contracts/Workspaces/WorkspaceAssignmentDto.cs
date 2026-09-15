using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceAssignmentDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public Guid SourceWorkspaceId { get; set; }
    public Guid TargetWorkspaceId { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
}
