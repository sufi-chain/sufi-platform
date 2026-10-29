using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.AuditLogging.Dtos;
using SufiChain.SufiPlatform.AuditLogging.Permissions;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.AuditLogging;

/// <summary>
/// Application service for managing audit logs.
/// </summary>
[Authorize(AuditLoggingPermissions.AuditLogs.Default)]
public class AuditLogAppService : SufiApplicationService, IAuditLogAppService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogAppService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public virtual async Task<PagedResultDto<AuditLogListItemDto>> GetListAsync(GetAuditLogListInput input)
    {
        var canViewOtherUsers = await CanViewOtherUsersAsync();
        var userId = AuditLogViewerScope.EffectiveUserId(canViewOtherUsers, CurrentUser.Id, input.UserId);
        var userName = AuditLogViewerScope.EffectiveUserName(canViewOtherUsers, input.UserName);

        var totalCount = await _auditLogRepository.GetCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            clientId: input.ClientId,
            userId: userId,
            userName: userName,
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
            userId: userId,
            userName: userName,
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
        if (!AuditLogViewerScope.CanReadEntry(await CanViewOtherUsersAsync(), CurrentUser.Id, auditLog.UserId))
        {
            throw new EntityNotFoundException(typeof(AuditLog), id);
        }

        return ObjectMapper.Map<AuditLog, AuditLogDto>(auditLog);
    }

    public virtual async Task<Dictionary<DateTime, double>> GetAverageExecutionDurationPerDayAsync(
        DateTime startDate, 
        DateTime endDate)
    {
        var canViewOtherUsers = await CanViewOtherUsersAsync();
        var userId = canViewOtherUsers
            ? null
            : AuditLogViewerScope.EffectiveUserId(false, CurrentUser.Id, null);
        return await _auditLogRepository.GetAverageExecutionDurationPerDayAsync(startDate, endDate, userId);
    }

    protected virtual async Task<bool> CanViewOtherUsersAsync()
    {
        return AuditLogViewerScope.CanViewOtherUsers(
            CurrentUser.IsInRole(AuditLogViewerScope.AdminRoleName),
            await AuthorizationService.IsGrantedAsync(AuditLoggingPermissions.AuditLogs.ViewAll));
    }
}
