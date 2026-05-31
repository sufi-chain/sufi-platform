using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiAbp.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/account/captcha")]
public class CaptchaController : SufiAbpControllerBase, ICaptchaAppService
{
    private readonly ICaptchaAppService _captchaAppService;

    public CaptchaController(ICaptchaAppService captchaAppService)
    {
        _captchaAppService = captchaAppService;
    }

    [HttpGet]
    [Route("challenge")]
    public virtual Task<CaptchaChallengeDto> GetChallengeAsync()
    {
        return _captchaAppService.GetChallengeAsync();
    }

    [HttpGet]
    [Route("options")]
    public virtual Task<CaptchaOptionsDto> GetOptionsAsync()
    {
        return _captchaAppService.GetOptionsAsync();
    }
}
