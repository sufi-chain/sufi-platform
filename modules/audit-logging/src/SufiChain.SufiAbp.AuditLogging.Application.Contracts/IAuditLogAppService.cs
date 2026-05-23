using SufiChain.SufiAbp.AuditLogging.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AuditLogging;

/// <summary>
/// Application service for managing audit logs.
/// </summary>
[RemoteService(Name = AuditLoggingRemoteServiceConsts.RemoteServiceName)]
public interface IAuditLogAppService : IApplicationService
{
    /// <summary>
    /// Gets a paged list of audit logs.
    /// </summary>
    Task<PagedResultDto<AuditLogListItemDto>> GetListAsync(GetAuditLogListInput input);

    /// <summary>
    /// Gets a specific audit log by ID.
    /// </summary>
    Task<AuditLogDto> GetAsync(Guid id);

    /// <summary>
    /// Gets average execution duration per day for a date range.
    /// </summary>
    Task<Dictionary<DateTime, double>> GetAverageExecutionDurationPerDayAsync(DateTime startDate, DateTime endDate);
}
