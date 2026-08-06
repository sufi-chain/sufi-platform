using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.AuditLogging.Dtos;

/// <summary>
/// Full entity change details DTO.
/// </summary>
public class EntityChangeDto : EntityDto<Guid>
{
    public Guid AuditLogId { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime ChangeTime { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public string? EntityTypeFullName { get; set; }
    public string? EntityId { get; set; }
    
    public List<EntityPropertyChangeDto> PropertyChanges { get; set; } = new();
    public Dictionary<string, object?>? ExtraProperties { get; set; }
}

/// <summary>
/// Entity change list item DTO with username for grid display.
/// </summary>
public class EntityChangeListItemDto :  EntityDto<Guid>
{
    public Guid AuditLogId { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime ChangeTime { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public string? EntityTypeFullName { get; set; }
    public string? EntityId { get; set; }
    public string? UserName { get; set; }
    
    public List<EntityPropertyChangeDto> PropertyChanges { get; set; } = new();
}

/// <summary>
/// Entity property change DTO.
/// </summary>
public class EntityPropertyChangeDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid EntityChangeId { get; set; }
    public string? PropertyName { get; set; }
    public string? PropertyTypeFullName { get; set; }
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}
