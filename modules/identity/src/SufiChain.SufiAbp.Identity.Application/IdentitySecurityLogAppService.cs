using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.Identity.Dtos;
using SufiChain.SufiAbp.Identity.Permissions;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity;

/// <summary>
/// Application service for managing identity security logs.
/// </summary>
[Authorize(IdentityPermissions.SecurityLogs.Default)]
public class IdentitySecurityLogAppService : ApplicationService, IIdentitySecurityLogAppService
{
    private readonly IIdentitySecurityLogRepository _securityLogRepository;

    public IdentitySecurityLogAppService(IIdentitySecurityLogRepository securityLogRepository)
    {
        _securityLogRepository = securityLogRepository;
    }

    public virtual async Task<PagedResultDto<SecurityLogListItemDto>> GetListAsync(GetSecurityLogListInput input)
    {
        var totalCount = await _securityLogRepository.GetCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            applicationName: input.ApplicationName,
            identity: input.Identity,
            action: input.Action,
            userId: input.UserId,
            userName: input.UserName,
            clientId: input.ClientId,
            correlationId: input.CorrelationId,
            clientIpAddress: input.ClientIpAddress
        );

        var securityLogs = await _securityLogRepository.GetListAsync(
            sorting: input.Sorting ?? "CreationTime DESC",
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            applicationName: input.ApplicationName,
            identity: input.Identity,
            action: input.Action,
            userId: input.UserId,
            userName: input.UserName,
            clientId: input.ClientId,
            correlationId: input.CorrelationId,
            clientIpAddress: input.ClientIpAddress
        );

        var items = ObjectMapper.Map<List<IdentitySecurityLog>, List<SecurityLogListItemDto>>(securityLogs);

        return new PagedResultDto<SecurityLogListItemDto>(totalCount, items);
    }

    public virtual async Task<SecurityLogDto> GetAsync(Guid id)
    {
        var securityLog = await _securityLogRepository.GetAsync(id);
        return ObjectMapper.Map<IdentitySecurityLog, SecurityLogDto>(securityLog);
    }
}
