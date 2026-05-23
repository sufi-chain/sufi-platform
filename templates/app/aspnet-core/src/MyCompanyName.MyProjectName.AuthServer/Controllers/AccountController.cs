using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.AspNetCore.Authentication;
using SufiChain.SufiAbp.AspNetCore.Authentication.Server.Controllers;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using SufiChain.SufiAbp.UI.MultiTenancy;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace MyCompanyName.MyProjectName.Controllers;

/// <summary>
/// AuthServer account controller. All SufiAbp login implementation (complete-login, OIDC Login/Logout) lives in <see cref="AccountController"/>.
/// </summary>
public class AccountController : AccountController
{
    public AccountController(
        IOptions<SufiAbpAuthenticationOptions> options,
        ILoginCompletionTokenStore tokenStore,
        SignInManager<IdentityUser> signInManager,
        IdentityUserManager userManager,
        IOptions<TenantSwitchOptions> tenantOptions,
        IAccountSecurityLogAppService securityLogAppService)
        : base(options, tokenStore, signInManager, userManager, tenantOptions, securityLogAppService)
    {
    }
}
