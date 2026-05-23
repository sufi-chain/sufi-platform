using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity.Dtos;

/// <summary>
/// Full security log details DTO.
/// </summary>
public class SecurityLogDto : SufiAbpEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? ApplicationName { get; set; }
    public string? Identity { get; set; }
    public string? Action { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ClientId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? BrowserInfo { get; set; }
    public DateTime CreationTime { get; set; }
    public Dictionary<string, object?>? ExtraProperties { get; set; }
}

/// <summary>
/// Security log list item DTO for grid display.
/// </summary>
public class SecurityLogListItemDto : SufiAbpEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? ApplicationName { get; set; }
    public string? Identity { get; set; }
    public string? Action { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ClientId { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? BrowserInfo { get; set; }
    public DateTime CreationTime { get; set; }
}
