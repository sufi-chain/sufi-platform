using SufiChain.SufiAbp.Identity.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Identity;

/// <summary>
/// Application service for managing identity security logs.
/// </summary>
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
public interface IIdentitySecurityLogAppService : IApplicationService
{
    /// <summary>
    /// Gets a paged list of security logs.
    /// </summary>
    Task<PagedResultDto<SecurityLogListItemDto>> GetListAsync(GetSecurityLogListInput input);

    /// <summary>
    /// Gets a specific security log by ID.
    /// </summary>
    Task<SecurityLogDto> GetAsync(Guid id);
}
