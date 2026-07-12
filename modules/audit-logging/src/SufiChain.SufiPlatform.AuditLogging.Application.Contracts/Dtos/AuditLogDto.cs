using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.AuditLogging.Dtos;

/// <summary>
/// Full audit log details DTO.
/// </summary>
public class AuditLogDto : EntityDto<Guid>
{
    public string? ApplicationName { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public Guid? ImpersonatorUserId { get; set; }
    public string? ImpersonatorUserName { get; set; }
    public Guid? ImpersonatorTenantId { get; set; }
    public string? ImpersonatorTenantName { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? ClientName { get; set; }
    public string? ClientId { get; set; }
    public string? CorrelationId { get; set; }
    public string? BrowserInfo { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? Exceptions { get; set; }
    public string? Comments { get; set; }
    
    public List<AuditLogActionDto> Actions { get; set; } = new();
    public List<EntityChangeDto> EntityChanges { get; set; } = new();
}

/// <summary>
/// Audit log list item DTO for grid display.
/// </summary>
public class AuditLogListItemDto : EntityDto<Guid>
{
    public string? ApplicationName { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? ClientName { get; set; }
    public string? BrowserInfo { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public bool HasException { get; set; }
    public string? Exceptions { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Audit log action DTO.
/// </summary>
public class AuditLogActionDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid AuditLogId { get; set; }
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public string? Parameters { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public Dictionary<string, object?>? ExtraProperties { get; set; }
}
