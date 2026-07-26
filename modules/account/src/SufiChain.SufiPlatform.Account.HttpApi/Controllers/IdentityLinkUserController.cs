using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace SufiChain.SufiPlatform.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/account/link-user")]
public class IdentityLinkUserController : SufiControllerBase, IIdentityLinkUserAppService
{
    private readonly IIdentityLinkUserAppService _linkUserAppService;

    public IdentityLinkUserController(IIdentityLinkUserAppService linkUserAppService)
    {
        _linkUserAppService = linkUserAppService;
    }

    [HttpGet]
    public virtual Task<ListResultDto<LinkUserDto>> GetAllListAsync()
    {
        return _linkUserAppService.GetAllListAsync();
    }

    [HttpPost]
    [Route("link")]
    public virtual Task LinkAsync(LinkUserInput input)
    {
        return _linkUserAppService.LinkAsync(input);
    }

    [HttpPost]
    [Route("unlink")]
    public virtual Task UnlinkAsync(UnLinkUserInput input)
    {
        return _linkUserAppService.UnlinkAsync(input);
    }

    [HttpPost]
    [Route("is-linked")]
    public virtual Task<bool> IsLinkedAsync(IsLinkedInput input)
    {
        return _linkUserAppService.IsLinkedAsync(input);
    }

    [HttpPost]
    [Route("generate-link-token")]
    public virtual Task<string> GenerateLinkTokenAsync()
    {
        return _linkUserAppService.GenerateLinkTokenAsync();
    }

    [HttpPost]
    [Route("verify-link-token")]
    public virtual Task<bool> VerifyLinkTokenAsync(VerifyLinkTokenInput input)
    {
        return _linkUserAppService.VerifyLinkTokenAsync(input);
    }

    [HttpPost]
    [Route("generate-link-login-token")]
    public virtual Task<string> GenerateLinkLoginTokenAsync()
    {
        return _linkUserAppService.GenerateLinkLoginTokenAsync();
    }

    [HttpPost]
    [Route("verify-link-login-token")]
    public virtual Task<bool> VerifyLinkLoginTokenAsync(VerifyLinkLoginTokenInput input)
    {
        return _linkUserAppService.VerifyLinkLoginTokenAsync(input);
    }
}
