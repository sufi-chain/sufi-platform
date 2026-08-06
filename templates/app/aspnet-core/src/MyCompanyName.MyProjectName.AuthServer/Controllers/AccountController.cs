using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.AspNetCore.Authentication;
using SufiChain.SufiPlatform.AspNetCore.Authentication.Server.Controllers;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace MyCompanyName.MyProjectName.Controllers;

/// <summary>
/// AuthServer account controller. All Sufi Platform login implementation (complete-login, OIDC Login/Logout) lives in <see cref="AccountController"/>.
/// </summary>
public class AccountController : AccountController
{
    public AccountController(
        IOptions<SufiAuthenticationOptions> options,
        ILoginCompletionTokenStore tokenStore,
        SignInManager<IdentityUser> signInManager,
        IdentityUserManager userManager,
        IOptions<TenantSwitchOptions> tenantOptions,
        IAccountSecurityLogAppService securityLogAppService)
        : base(options, tokenStore, signInManager, userManager, tenantOptions, securityLogAppService)
    {
    }
}
