using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AuditLogging.Dtos;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.AuditLogging.Controllers;

/// <summary>
/// Controller for audit log operations.
/// </summary>
[Area(AuditLoggingRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AuditLoggingRemoteServiceConsts.RemoteServiceName)]
[Route("api/audit-logging/audit-logs")]
public class AuditLogController : SufiControllerBase, IAuditLogAppService
{
    private readonly IAuditLogAppService _auditLogAppService;

    public AuditLogController(IAuditLogAppService auditLogAppService)
    {
        _auditLogAppService = auditLogAppService;
    }

    /// <summary>
    /// Gets a paged list of audit logs.
    /// </summary>
    [HttpGet]
    public virtual Task<PagedResultDto<AuditLogListItemDto>> GetListAsync([FromQuery] GetAuditLogListInput input)
    {
        return _auditLogAppService.GetListAsync(input);
    }

    /// <summary>
    /// Gets a specific audit log by ID.
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    public virtual Task<AuditLogDto> GetAsync(Guid id)
    {
        return _auditLogAppService.GetAsync(id);
    }

    /// <summary>
    /// Gets average execution duration per day for a date range.
    /// </summary>
    [HttpGet]
    [Route("average-execution-duration-per-day")]
    public virtual Task<Dictionary<DateTime, double>> GetAverageExecutionDurationPerDayAsync(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        return _auditLogAppService.GetAverageExecutionDurationPerDayAsync(startDate, endDate);
    }
}
