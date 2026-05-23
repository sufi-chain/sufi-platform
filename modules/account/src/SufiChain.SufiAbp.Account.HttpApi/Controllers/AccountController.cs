using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/account")]
public class AccountController : SufiAbpControllerBase, IAccountAppService
{
    private readonly IAccountAppService _accountAppService;

    public AccountController(IAccountAppService accountAppService)
    {
        _accountAppService = accountAppService;
    }

    [HttpPost]
    [Route("register")]
    public virtual Task<IdentityUserDto> RegisterAsync(RegisterDto input)
    {
        return _accountAppService.RegisterAsync(input);
    }

    [HttpPost]
    [Route("send-password-reset-code")]
    public virtual Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        return _accountAppService.SendPasswordResetCodeAsync(input);
    }

    [HttpPost]
    [Route("verify-password-reset-token")]
    public virtual Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input)
    {
        return _accountAppService.VerifyPasswordResetTokenAsync(input);
    }

    [HttpPost]
    [Route("reset-password")]
    public virtual Task ResetPasswordAsync(
        ResetPasswordDto input)
    {
        return _accountAppService.ResetPasswordAsync(input);
    }
}
