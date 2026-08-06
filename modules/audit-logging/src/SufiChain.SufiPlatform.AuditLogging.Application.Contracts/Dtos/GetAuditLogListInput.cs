using System.Net;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.AuditLogging.Dtos;

/// <summary>
/// Input DTO for getting a paged list of audit logs.
/// </summary>
public class GetAuditLogListInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public string? ClientId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ApplicationName { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public int? MaxExecutionDuration { get; set; }
    public int? MinExecutionDuration { get; set; }
    public bool? HasException { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public bool IncludeDetails { get; set; }
}
