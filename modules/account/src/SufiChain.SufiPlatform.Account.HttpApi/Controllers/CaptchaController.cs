using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/account/captcha")]
public class CaptchaController : SufiControllerBase, ICaptchaAppService
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
