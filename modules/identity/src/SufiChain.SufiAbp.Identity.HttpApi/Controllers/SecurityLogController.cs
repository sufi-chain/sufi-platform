using Microsoft.AspNetCore.Mvc;
using global::SufiChain.SufiAbp.Identity;
using global::SufiChain.SufiAbp.Identity.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.Identity.Controllers;

/// <summary>
/// Controller for security log operations.
/// </summary>
[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/identity/security-logs")]
public class SecurityLogController : SufiAbpControllerBase, IIdentitySecurityLogAppService
{
    private readonly IIdentitySecurityLogAppService _securityLogAppService;

    public SecurityLogController(IIdentitySecurityLogAppService securityLogAppService)
    {
        _securityLogAppService = securityLogAppService;
    }

    /// <summary>
    /// Gets a paged list of security logs.
    /// </summary>
    [HttpGet]
    public virtual Task<PagedResultDto<SecurityLogListItemDto>> GetListAsync([FromQuery] GetSecurityLogListInput input)
    {
        return _securityLogAppService.GetListAsync(input);
    }

    /// <summary>
    /// Gets a specific security log by ID.
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    public virtual Task<SecurityLogDto> GetAsync(Guid id)
    {
        return _securityLogAppService.GetAsync(id);
    }
}
