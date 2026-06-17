using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.AspNetCore.Authentication;
using SufiChain.SufiAbp.AspNetCore.Authentication.Server.Controllers;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using SufiChain.SufiAbp.UI.MultiTenancy;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.AspNetCore;
using IdentityUser = SufiChain.SufiAbp.Identity.IdentityUser;

namespace MyCompanyName.MyProjectName.Controllers;

/// <summary>
/// Account controller for integrated Blazor WebApp.
/// All SufiAbp login implementation (complete-login, logout, tenant switch) lives in <see cref="SpAccountController"/>.
/// Login is handled by the Blazor Account.Blazor pages directly (local cookie auth).
/// </summary>
public class AccountController : SufiAbpAccountController
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
