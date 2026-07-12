using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/account/two-factor")]
public class AccountTwoFactorController : SufiControllerBase, IAccountTwoFactorAppService
{
    private readonly IAccountTwoFactorAppService _twoFactorAppService;

    public AccountTwoFactorController(IAccountTwoFactorAppService twoFactorAppService)
    {
        _twoFactorAppService = twoFactorAppService;
    }

    [HttpGet]
    [Route("login-options")]
    public virtual Task<TwoFactorLoginOptionsDto> GetLoginOptionsAsync()
    {
        return _twoFactorAppService.GetLoginOptionsAsync();
    }

    [HttpGet]
    [Route("info")]
    public virtual Task<TwoFactorInfoDto> GetTwoFactorInfoAsync()
    {
        return _twoFactorAppService.GetTwoFactorInfoAsync();
    }

    [HttpPost]
    [Route("authenticator-setup")]
    public virtual Task<AuthenticatorSetupDto> GenerateAuthenticatorSetupAsync()
    {
        return _twoFactorAppService.GenerateAuthenticatorSetupAsync();
    }

    [HttpPost]
    [Route("enable")]
    public virtual Task<RecoveryCodesDto> EnableTwoFactorAsync(EnableTwoFactorInput input)
    {
        return _twoFactorAppService.EnableTwoFactorAsync(input);
    }

    [HttpPost]
    [Route("disable")]
    public virtual Task DisableTwoFactorAsync(DisableTwoFactorInput input)
    {
        return _twoFactorAppService.DisableTwoFactorAsync(input);
    }

    [HttpPost]
    [Route("recovery-codes")]
    public virtual Task<RecoveryCodesDto> GenerateRecoveryCodesAsync()
    {
        return _twoFactorAppService.GenerateRecoveryCodesAsync();
    }

    [HttpPost]
    [Route("send-code")]
    public virtual Task SendTwoFactorCodeAsync(SendTwoFactorCodeInput input)
    {
        return _twoFactorAppService.SendTwoFactorCodeAsync(input);
    }

    [HttpPost]
    [Route("complete-login")]
    public virtual Task<CompleteTwoFactorLoginResultDto> CompleteTwoFactorLoginAsync(
        CompleteTwoFactorLoginInput input)
    {
        return _twoFactorAppService.CompleteTwoFactorLoginAsync(input);
    }

    [HttpGet]
    [Route("post-login-redirect")]
    public virtual Task<string?> GetPostLoginRedirectUrlAsync([FromQuery] Guid userId, [FromQuery] string? returnUrl)
    {
        return _twoFactorAppService.GetPostLoginRedirectUrlAsync(userId, returnUrl);
    }
}
