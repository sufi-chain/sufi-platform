using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.AspNetCore.Authentication;
using SufiChain.SufiPlatform.AspNetCore.Authentication.Server.Controllers;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.AspNetCore;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace MyCompanyName.MyProjectName.Controllers;

/// <summary>
/// Account controller for integrated Blazor WebApp.
/// All Sufi Platform login implementation (complete-login, logout, tenant switch) lives in <see cref="SpAccountController"/>.
/// Login is handled by the Blazor Account.Blazor pages directly (local cookie auth).
/// </summary>
public class AccountController : SufiAccountController
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
