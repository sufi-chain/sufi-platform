using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.AuditLogging.Dtos;
using SufiChain.SufiAbp.AuditLogging.Permissions;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;
using SufiChain.SufiAbp.AuditLogging;

namespace SufiChain.SufiAbp.AuditLogging;

/// <summary>
/// Application service for managing audit logs.
/// </summary>
[Authorize(AuditLoggingPermissions.AuditLogs.Default)]
public class AuditLogAppService : ApplicationService, IAuditLogAppService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogAppService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public virtual async Task<PagedResultDto<AuditLogListItemDto>> GetListAsync(GetAuditLogListInput input)
    {
        var totalCount = await _auditLogRepository.GetCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            clientId: input.ClientId,
            userId: input.UserId,
            userName: input.UserName,
            applicationName: input.ApplicationName,
            clientIpAddress: input.ClientIpAddress,
            correlationId: input.CorrelationId,
            maxExecutionDuration: input.MaxExecutionDuration,
            minExecutionDuration: input.MinExecutionDuration,
            hasException: input.HasException,
            httpStatusCode: input.HttpStatusCode
        );

        var auditLogs = await _auditLogRepository.GetListAsync(
            sorting: input.Sorting ?? "ExecutionTime DESC",
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            clientId: input.ClientId,
            userId: input.UserId,
            userName: input.UserName,
            applicationName: input.ApplicationName,
            clientIpAddress: input.ClientIpAddress,
            correlationId: input.CorrelationId,
            maxExecutionDuration: input.MaxExecutionDuration,
            minExecutionDuration: input.MinExecutionDuration,
            hasException: input.HasException,
            httpStatusCode: input.HttpStatusCode,
            includeDetails: input.IncludeDetails
        );

        var items = ObjectMapper.Map<List<AuditLog>, List<AuditLogListItemDto>>(auditLogs);

        return new PagedResultDto<AuditLogListItemDto>(totalCount, items);
    }

    public virtual async Task<AuditLogDto> GetAsync(Guid id)
    {
        var auditLog = await _auditLogRepository.GetAsync(id);
        return ObjectMapper.Map<AuditLog, AuditLogDto>(auditLog);
    }

    public virtual async Task<Dictionary<DateTime, double>> GetAverageExecutionDurationPerDayAsync(
        DateTime startDate, 
        DateTime endDate)
    {
        return await _auditLogRepository.GetAverageExecutionDurationPerDayAsync(startDate, endDate);
    }
}
