using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.Identity;
using Volo.Abp;

namespace SufiChain.SufiAbp.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/account/otp")]
public class AccountOtpController : SufiAbpControllerBase, IAccountOtpAppService
{
    private readonly IAccountOtpAppService _otpAppService;

    public AccountOtpController(IAccountOtpAppService otpAppService)
    {
        _otpAppService = otpAppService;
    }

    [HttpGet]
    [Route("options")]
    public virtual Task<OtpOptionsDto> GetOtpOptionsAsync()
    {
        return _otpAppService.GetOtpOptionsAsync();
    }

    [HttpPost]
    [Route("send-login")]
    public virtual Task SendLoginOtpAsync(SendOtpInput input)
    {
        return _otpAppService.SendLoginOtpAsync(input);
    }

    [HttpPost]
    [Route("verify-login")]
    public virtual Task<VerifyLoginOtpResultDto> VerifyLoginOtpAsync(VerifyLoginOtpInput input)
    {
        return _otpAppService.VerifyLoginOtpAsync(input);
    }

    [HttpPost]
    [Route("send-registration")]
    public virtual Task SendRegistrationOtpAsync(SendOtpInput input)
    {
        return _otpAppService.SendRegistrationOtpAsync(input);
    }

    [HttpPost]
    [Route("verify-registration")]
    public virtual Task<VerifyRegistrationOtpResultDto> VerifyRegistrationOtpAsync(VerifyOtpInput input)
    {
        return _otpAppService.VerifyRegistrationOtpAsync(input);
    }

    [HttpPost]
    [Route("register")]
    public virtual Task<IdentityUserDto> RegisterWithOtpAsync(RegisterWithOtpDto input)
    {
        return _otpAppService.RegisterWithOtpAsync(input);
    }
}
