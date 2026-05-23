using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.AuditLogging.Dtos;

/// <summary>
/// Input DTO for getting a paged list of entity changes.
/// </summary>
public class GetEntityChangeListInput : PagedAndSortedResultRequestDto
{
    public Guid? AuditLogId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public EntityChangeType? ChangeType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityTypeFullName { get; set; }
    public bool IncludeDetails { get; set; }
}
