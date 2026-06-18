using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using SufiChain.SufiAbp.UI.MultiTenancy;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using IdentityUser = SufiChain.SufiAbp.Identity.IdentityUser;

namespace SufiChain.SufiAbp.AspNetCore.Authentication.Server.Controllers;

/// <summary>
/// Base MVC controller for SufiAbp account flows: OIDC (Login/Logout) and cookie-based complete-login
/// for Blazor Interactive Server. Hosts inherit from this and register it so all SufiAbp login implementation
/// lives in the framework.
/// </summary>
[Route("Account")]
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class SufiAbpAccountController : AbpController
{
    private readonly SufiAbpAuthenticationOptions _options;
    private readonly ILoginCompletionTokenStore _tokenStore;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IdentityUserManager _userManager;
    private readonly TenantSwitchOptions _tenantOptions;
    private readonly IAccountSecurityLogAppService _securityLogAppService;

    protected SufiAbpAccountController(
        IOptions<SufiAbpAuthenticationOptions> options,
        ILoginCompletionTokenStore tokenStore,
        SignInManager<IdentityUser> signInManager,
        IdentityUserManager userManager,
        IOptions<TenantSwitchOptions> tenantOptions,
        IAccountSecurityLogAppService securityLogAppService)
    {
        _options = options.Value;
        _tokenStore = tokenStore;
        _signInManager = signInManager;
        _userManager = userManager;
        _tenantOptions = tenantOptions.Value;
        _securityLogAppService = securityLogAppService;
    }

    /// <summary>
    /// Initiates OIDC login challenge.
    /// Route is /Account/OidcLogin to avoid conflict with Blazor login page at /account/login.
    /// Only effective when <see cref="SufiAbpAuthenticationOptions.UseOidcClientFlow"/> is true.
    /// </summary>
    /// <param name="returnUrl">URL to return to after successful authentication.</param>
    [HttpGet("OidcLogin")]
    public IActionResult OidcLogin(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        // If already authenticated, redirect to return URL
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl);
        }

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = returnUrl
            },
            _options.OidcChallengeScheme);
    }

    /// <summary>
    /// Handles logout.
    /// When <see cref="SufiAbpAuthenticationOptions.UseOidcClientFlow"/> is true (tiered WebApp client):
    ///   signs out of cookie + OIDC scheme (triggers redirect to IdP logout endpoint).
    /// When false (AuthServer / non-tiered all-in-one):
    ///   signs out of ASP.NET Core Identity's ApplicationScheme cookie and redirects locally.
    /// </summary>
    /// <param name="returnUrl">URL to return to after logout.</param>
    [HttpGet("Logout")]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var userName = User.Identity?.Name;

        if (_options.UseOidcClientFlow)
        {
            await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.Identity, IdentitySecurityLogActionConsts.Logout, userName);
            // Tiered: sign out of cookie + OIDC (triggers redirect to IdP logout endpoint)
            await HttpContext.SignOutAsync(_options.CookieScheme);
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = returnUrl
                },
                _options.OidcChallengeScheme);
        }

        await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.Identity, IdentitySecurityLogActionConsts.Logout, userName);
        // Non-tiered / AuthServer: sign out of ASP.NET Core Identity cookie
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Initiates external login challenge (Google, Microsoft, Facebook, etc.).
    /// Redirects to provider; callback is <see cref="ExternalLoginCallback"/>.
    /// </summary>
    /// <param name="provider">The external provider name (e.g. "Google", "Microsoft").</param>
    /// <param name="returnUrl">URL to return to after successful authentication.</param>
    [HttpPost("ExternalLogin")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var callbackUrl = Url.Action(
            nameof(ExternalLoginCallback),
            "Account",
            new { returnUrl },
            Request.Scheme);
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Handles the callback from external login providers.
    /// If user exists with external login -> sign in and redirect.
    /// If user exists by email but no external login -> link and sign in.
    /// If new user -> redirect to Register with IsExternalLogin.
    /// </summary>
    /// <param name="returnUrl">URL to return to after successful authentication.</param>
    /// <param name="remoteError">Error from the external provider, if any.</param>
    [HttpGet("ExternalLoginCallback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= "/";
        if (!Url.IsLocalUrl(returnUrl))
            returnUrl = "/";

        if (!string.IsNullOrEmpty(remoteError))
        {
            Logger.LogWarning("External login callback error: {RemoteError}", remoteError);
            return Redirect($"/account/login?error={Uri.EscapeDataString(remoteError)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
        {
            Logger.LogWarning("External login info is not available");
            return Redirect($"/account/login?error=ExternalLoginInfoNotAvailable&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            loginInfo.LoginProvider,
            loginInfo.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (result.IsLockedOut)
        {
            Logger.LogWarning("External login callback: user is locked out");
            throw new UserFriendlyException("Cannot proceed because user is locked out!");
        }

        if (result.IsNotAllowed)
        {
            Logger.LogWarning("External login callback: user is not allowed");
            throw new UserFriendlyException("Cannot proceed because user is not allowed!");
        }

        if (result.Succeeded)
        {
            await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.IdentityExternal, IdentitySecurityLogActionConsts.LoginSucceeded, null);
            return LocalRedirect(returnUrl);
        }

        var email = loginInfo.Principal.FindFirstValue(AbpClaimTypes.Email) ?? loginInfo.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Redirect($"/account/register?isExternalLogin=true&externalLoginAuthSchema={Uri.EscapeDataString(loginInfo.LoginProvider)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var emailParam = !string.IsNullOrEmpty(email) ? $"&email={Uri.EscapeDataString(email)}" : "";
            return Redirect($"/account/register?isExternalLogin=true&externalLoginAuthSchema={Uri.EscapeDataString(loginInfo.LoginProvider)}&returnUrl={Uri.EscapeDataString(returnUrl)}{emailParam}");
        }

        if (await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey) == null)
        {
            var addResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addResult.Succeeded)
            {
                var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                throw new UserFriendlyException($"Failed to link external login: {errors}");
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.IdentityExternal, IdentitySecurityLogActionConsts.LoginSucceeded, user.UserName);

        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Completes external registration: creates user from external login info and signs in.
    /// Called when a new user arrives from ExternalLoginCallback redirect to Register page.
    /// </summary>
    /// <param name="userName">Desired username.</param>
    /// <param name="emailAddress">Email from external provider.</param>
    /// <param name="externalLoginAuthSchema">Provider name (e.g. "Google").</param>
    /// <param name="returnUrl">URL to redirect after successful registration.</param>
    [HttpPost("ExternalLoginConfirmation")]
    public async Task<IActionResult> ExternalLoginConfirmation(
        string userName,
        string emailAddress,
        string externalLoginAuthSchema,
        string? returnUrl = null)
    {
        returnUrl ??= "/";
        if (!Url.IsLocalUrl(returnUrl))
            returnUrl = "/";

        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
        {
            Logger.LogWarning("External login info is not available for confirmation");
            return Redirect($"/account/login?error=ExternalLoginInfoNotAvailable&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        if (string.IsNullOrWhiteSpace(userName))
            userName = await _userManager.GetUserNameFromEmailAsync(emailAddress);
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return Redirect($"/account/register?isExternalLogin=true&externalLoginAuthSchema={Uri.EscapeDataString(externalLoginAuthSchema)}&returnUrl={Uri.EscapeDataString(returnUrl)}&error=EmailRequired");
        }

        var user = new IdentityUser(GuidGenerator.Create(), userName.Trim(), emailAddress.Trim(), CurrentTenant.Id);
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Redirect($"/account/register?isExternalLogin=true&externalLoginAuthSchema={Uri.EscapeDataString(externalLoginAuthSchema)}&returnUrl={Uri.EscapeDataString(returnUrl)}&email={Uri.EscapeDataString(emailAddress)}&error={Uri.EscapeDataString(errors)}");
        }

        var addDefaultRolesResult = await _userManager.AddDefaultRolesAsync(user);
        if (!addDefaultRolesResult.Succeeded)
        {
            var errors = string.Join("; ", addDefaultRolesResult.Errors.Select(e => e.Description));
            Logger.LogWarning("Failed to add default roles: {Errors}", errors);
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
        if (!addLoginResult.Succeeded)
        {
            var errors = string.Join("; ", addLoginResult.Errors.Select(e => e.Description));
            throw new UserFriendlyException($"Failed to link external login: {errors}");
        }

        await _signInManager.SignInAsync(user, isPersistent: true, externalLoginAuthSchema);
        await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.IdentityExternal, IdentitySecurityLogActionConsts.LoginSucceeded, user.UserName);

        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Handles access denied scenarios.
    /// </summary>
    /// <param name="returnUrl">The URL that was denied.</param>
    [HttpGet("AccessDenied")]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        // You can customize this to redirect to an access denied page
        // or show a specific error view
        return Forbid();
    }

    /// <summary>
    /// Handles front-channel logout from the identity provider.
    /// Called by IdP when user logs out from another application.
    /// Only effective when <see cref="SufiAbpAuthenticationOptions.UseOidcClientFlow"/> is true.
    /// </summary>
    [HttpGet("FrontChannelLogout")]
    public async Task<IActionResult> FrontChannelLogout()
    {
        if (_options.UseOidcClientFlow)
        {
            // Sign out of local OIDC client cookie
            await HttpContext.SignOutAsync(_options.CookieScheme);
        }
        else
        {
            // Non-tiered / AuthServer: sign out of Identity cookie
            await _signInManager.SignOutAsync();
        }

        return NoContent();
    }

    /// <summary>
    /// Consumes a one-time login token (from Blazor circuit after successful PasswordSignIn),
    /// signs in the user in this HTTP request so the auth cookie is set, then redirects.
    /// Used by hosts that run Blazor account UI with Interactive Server.
    /// </summary>
    [HttpGet]
    [Route("/account/complete-login")]
    public async Task<IActionResult> CompleteLogin([FromQuery] string? token, [FromQuery] string? returnUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Redirect($"/account/login?error=InvalidOrExpiredToken&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        var consumed = await _tokenStore.ConsumeAsync(token, cancellationToken);
        if (consumed == null)
        {
            return Redirect($"/account/login?error=InvalidOrExpiredToken&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        var (userId, redirectUrl, rememberMe) = consumed.Value;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Redirect($"/account/login?error=UserNotFound&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        await _signInManager.SignInAsync(user, rememberMe);

        await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.Identity, IdentitySecurityLogActionConsts.LoginSucceeded, user.UserName);

        var twoFactorAppService = HttpContext.RequestServices.GetService<IAccountTwoFactorAppService>();
        if (twoFactorAppService != null)
        {
            var enforceUrl = await twoFactorAppService.GetPostLoginRedirectUrlAsync(userId, redirectUrl);
            if (!string.IsNullOrEmpty(enforceUrl) && Url.IsLocalUrl(enforceUrl))
            {
                return Redirect(enforceUrl);
            }
        }

        var target = !string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl) ? redirectUrl : "/";
        return Redirect(target);
    }

    /// <summary>
    /// Sets the tenant cookie via HTTP response and redirects. Used by Blazor UI to switch tenant
    /// without JavaScript interop (avoids prerender issues). Route: /Account/SwitchTenant
    /// When a tenant name is provided (instead of an ID), the name is resolved to a GUID
    /// via <see cref="ITenantStore"/> so ABP's cookie resolver can parse it directly.
    /// </summary>
    [HttpGet("SwitchTenant")]
    public async Task<IActionResult> SwitchTenant([FromQuery] Guid? tenantId, [FromQuery] string? tenantName, [FromQuery] string? returnUrl)
    {
        returnUrl ??= Url.Content("~/");
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        var cookieName = _tenantOptions.TenantCookieName;
        if (!string.IsNullOrEmpty(cookieName))
        {
            string value;

            if (tenantId.HasValue)
            {
                value = tenantId.Value.ToString();
            }
            else if (!string.IsNullOrEmpty(tenantName))
            {
                // Resolve tenant name → GUID so ABP's CookieTenantResolveContributor
                // can parse the cookie value as a GUID directly (avoids normalization issues).
                var resolvedId = await ResolveTenantIdByNameAsync(tenantName);
                value = resolvedId?.ToString() ?? tenantName;
            }
            else
            {
                value = string.Empty;
            }


            Response.Cookies.Append(cookieName, value, new CookieOptions
            {
                Path = "/",
                SameSite = SameSiteMode.Lax,
                HttpOnly = false, // Cookie must be readable by JS for some scenarios; tenant cookie is not sensitive
                Secure = Request.IsHttps
            });
        }

        return LocalRedirect(returnUrl);
    }

    private async Task<Guid?> ResolveTenantIdByNameAsync(string tenantName)
    {
        var tenantStore = HttpContext.RequestServices.GetService<ITenantStore>();
        if (tenantStore == null)
        {
            return null;
        }

        try
        {
            var normalizedName = tenantName.ToUpperInvariant();
            var tenantConfig = await tenantStore.FindAsync(normalizedName);
            return tenantConfig?.Id;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving tenant name '{TenantName}' via ITenantStore", tenantName);
            return null;
        }
    }
}
