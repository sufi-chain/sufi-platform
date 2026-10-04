using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Settings;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.Server.Controllers;

/// <summary>
/// Base MVC controller for Sufi account flows: OIDC (Login/Logout) and cookie-based complete-login
/// for Blazor Interactive Server. Hosts inherit from this and register it so all Sufi login implementation
/// lives in the framework.
/// </summary>
[Route("Account")]
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class SufiAccountController : AbpController
{
    protected const string DefaultAuthenticatedReturnPath = "/panel/dashboard";
    protected const string DefaultLogoutReturnPath = "/account/login";

    private readonly SufiAuthenticationOptions _options;
    private readonly ILoginCompletionTokenStore _tokenStore;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IdentityUserManager _userManager;
    private readonly TenantSwitchOptions _tenantOptions;
    private readonly IAccountSecurityLogAppService _securityLogAppService;

    protected SufiAccountController(
        IOptions<SufiAuthenticationOptions> options,
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
    /// Only effective when <see cref="SufiAuthenticationOptions.UseOidcClientFlow"/> is true.
    /// </summary>
    /// <param name="returnUrl">URL to return to after successful authentication.</param>
    [HttpGet("OidcLogin")]
    public IActionResult OidcLogin(string? returnUrl = null)
    {
        returnUrl = NormalizeReturnUrl(returnUrl);

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
    /// When <see cref="SufiAuthenticationOptions.UseOidcClientFlow"/> is true (tiered WebApp client):
    ///   signs out of cookie + OIDC scheme (triggers redirect to IdP logout endpoint).
    /// When false (AuthServer / non-tiered all-in-one):
    ///   signs out of ASP.NET Core Identity's ApplicationScheme cookie and redirects locally.
    /// </summary>
    /// <param name="returnUrl">URL to return to after logout.</param>
    [HttpGet("Logout")]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        returnUrl ??= DefaultLogoutReturnPath;
        if (!Url.IsLocalUrl(returnUrl) || IsRootPath(returnUrl))
        {
            returnUrl = DefaultLogoutReturnPath;
        }

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
        returnUrl = NormalizeReturnUrl(returnUrl);
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
        returnUrl = NormalizeReturnUrl(returnUrl);

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
        returnUrl = NormalizeReturnUrl(returnUrl);

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

        var settingProvider = HttpContext.RequestServices.GetRequiredService<ISettingProvider>();
        if (await IdentityPhoneConfirmationRules.IsRequiredForRegistrationAsync(settingProvider))
        {
            return await RedirectToPhoneConfirmationAsync(user);
        }

        await _signInManager.SignInAsync(user, isPersistent: true, externalLoginAuthSchema);
        await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.IdentityExternal, IdentitySecurityLogActionConsts.LoginSucceeded, user.UserName);

        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Handles front-channel logout from the identity provider.
    /// Called by IdP when user logs out from another application.
    /// Only effective when <see cref="SufiAuthenticationOptions.UseOidcClientFlow"/> is true.
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
            Logger.LogWarning("Complete login rejected. Reason {Reason}.", "InvalidOrExpiredToken");
            return Redirect($"/account/login?error=LoginTokenExpired&returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(returnUrl))}");
        }

        var consumed = await _tokenStore.ConsumeAsync(token, cancellationToken);
        if (consumed == null)
        {
            Logger.LogWarning("Complete login rejected. Reason {Reason}.", "InvalidOrExpiredToken");
            return Redirect($"/account/login?error=LoginTokenExpired&returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(returnUrl))}");
        }

        var (userId, redirectUrl, rememberMe) = consumed.Value;
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            Logger.LogWarning("Complete login rejected. Reason {Reason} UserId {UserId}.", "UserNotFound", userId);
            return Redirect($"/account/login?error=LoginTokenExpired&returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(returnUrl))}");
        }

        if (!user.IsActive)
        {
            Logger.LogWarning("Complete login rejected. Reason {Reason} UserId {UserId}.", "Inactive", user.Id);
            return Redirect($"/account/login?error=LoginAccountInactive&returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(returnUrl))}");
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            var reason = emailConfirmed ? "LoginAccountInactive" : "EmailConfirmationRequired";
            try
            {
                await _securityLogAppService.SaveLoginEventAsync(
                    IdentitySecurityLogIdentityConsts.Identity,
                    IdentitySecurityLogActionConsts.LoginNotAllowed,
                    user.UserName);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    ex,
                    "Complete login security log was not saved. Reason {Reason} UserId {UserId}.",
                    ex.GetType().Name,
                    user.Id);
            }

            Logger.LogWarning("Complete login rejected. Reason {Reason} UserId {UserId}.", reason, user.Id);
            return Redirect($"/account/login?error={reason}&returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(returnUrl))}");
        }

        var settingProvider = HttpContext.RequestServices.GetRequiredService<ISettingProvider>();
        if (await IdentityPhoneConfirmationRules.IsRequiredForRegistrationAsync(settingProvider) &&
            !user.PhoneNumberConfirmed)
        {
            return await RedirectToPhoneConfirmationAsync(user);
        }

        try
        {
            await _signInManager.SignInAsync(user, rememberMe);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Complete login could not issue the cookie. Reason {Reason} UserId {UserId}.",
                ex.GetType().Name,
                user.Id);
            return Redirect($"/account/login?error=LoginAccountStoreUnavailable&returnUrl={Uri.EscapeDataString(NormalizeReturnUrl(returnUrl))}");
        }

        try
        {
            await _securityLogAppService.SaveLoginEventAsync(IdentitySecurityLogIdentityConsts.Identity, IdentitySecurityLogActionConsts.LoginSucceeded, user.UserName);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(
                ex,
                "Complete login security log was not saved. Reason {Reason} UserId {UserId}.",
                ex.GetType().Name,
                user.Id);
        }

        var twoFactorAppService = HttpContext.RequestServices.GetService<IAccountTwoFactorAppService>();
        if (twoFactorAppService != null)
        {
            var enforceUrl = await twoFactorAppService.GetPostLoginRedirectUrlAsync(userId, redirectUrl);
            if (!string.IsNullOrEmpty(enforceUrl) && Url.IsLocalUrl(enforceUrl))
            {
                return Redirect(enforceUrl);
            }
        }

        var target = !string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl)
            ? redirectUrl
            : DefaultAuthenticatedReturnPath;
        if (user.ShouldChangePasswordOnNextLogin)
        {
            return Redirect("/panel/portal/profile?tab=password");
        }

        if (IsRootPath(target))
        {
            target = DefaultAuthenticatedReturnPath;
        }

        return Redirect(target);
    }

    /// <summary>
    /// Switches the authenticated session to a linked user (full re-login, not impersonation).
    /// Verifies the source login token, confirms the link, sets the target tenant cookie, then signs in.
    /// </summary>
    [HttpGet]
    [Route("/account/link-login")]
    public async Task<IActionResult> LinkLogin(
        [FromQuery] Guid sourceLinkUserId,
        [FromQuery] Guid? sourceLinkTenantId,
        [FromQuery] string? sourceLinkToken,
        [FromQuery] Guid targetLinkUserId,
        [FromQuery] Guid? targetLinkTenantId,
        [FromQuery] string? returnUrl)
    {
        returnUrl = NormalizeReturnUrl(returnUrl);
        var sourceInfo = new IdentityLinkUserInfo(sourceLinkUserId, sourceLinkTenantId);
        var targetInfo = new IdentityLinkUserInfo(targetLinkUserId, targetLinkTenantId);
        var linkUserManager = HttpContext.RequestServices.GetRequiredService<IdentityLinkUserManager>();

        if (string.IsNullOrWhiteSpace(sourceLinkToken))
        {
            RejectLinkLogin(sourceInfo, targetInfo, LinkLoginRejectionReasons.MissingToken);
            return Redirect($"/account/login?error=LinkLoginFailed&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        bool tokenValid;
        try
        {
            tokenValid = await linkUserManager.VerifyLinkTokenAsync(
                sourceInfo,
                sourceLinkToken,
                LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose);
        }
        catch (Exception ex) when (ex is EntityNotFoundException)
        {
            RejectLinkLogin(sourceInfo, targetInfo, LinkLoginRejectionReasons.SourceUserNotFound);
            return Redirect($"/account/login?error=LinkLoginFailed&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        catch (Exception)
        {
            RejectLinkLogin(sourceInfo, targetInfo, LinkLoginRejectionReasons.TokenInvalid);
            return Redirect($"/account/login?error=LinkLoginFailed&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        if (!tokenValid)
        {
            var reason = await DescribeTokenFailureAsync(sourceInfo, sourceLinkToken);
            RejectLinkLogin(sourceInfo, targetInfo, reason);
            return Redirect($"/account/login?error=LinkLoginFailed&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        if (!await linkUserManager.IsLinkedAsync(sourceInfo, targetInfo))
        {
            var reason = await linkUserManager.DescribeMissingDirectLinkAsync(sourceInfo, targetInfo);
            RejectLinkLogin(sourceInfo, targetInfo, reason);
            return Redirect($"/account/login?error=LinkLoginFailed&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        await SetTenantCookieAsync(targetLinkTenantId, tenantName: null);

        IdentityUser? targetUser;
        using (CurrentTenant.Change(targetLinkTenantId))
        {
            targetUser = await _userManager.FindByIdAsync(targetLinkUserId.ToString());
            if (targetUser == null)
            {
                RejectLinkLogin(sourceInfo, targetInfo, LinkLoginRejectionReasons.TargetUserNotFound);
                return Redirect($"/account/login?error=UserNotFound&returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            await _signInManager.SignInAsync(targetUser, isPersistent: false);
        }

        await _securityLogAppService.SaveLoginEventAsync(
            IdentitySecurityLogIdentityConsts.Identity,
            IdentitySecurityLogActionConsts.LoginSucceeded,
            targetUser.UserName);

        return LocalRedirect(returnUrl);
    }

    protected virtual void RejectLinkLogin(
        IdentityLinkUserInfo source,
        IdentityLinkUserInfo target,
        string reason)
    {
        LinkLoginRejectionLog.Write(
            Logger,
            reason,
            source.UserId,
            source.TenantId,
            target.UserId,
            target.TenantId);
    }

    protected virtual async Task<string> DescribeTokenFailureAsync(
        IdentityLinkUserInfo source,
        string sourceLinkToken)
    {
        var describer = HttpContext.RequestServices.GetService<ILinkLoginTokenFailureDescriber>();
        if (describer == null)
        {
            return LinkLoginRejectionReasons.TokenInvalid;
        }

        try
        {
            var reason = await describer.DescribeAsync(
                source,
                sourceLinkToken,
                LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose);
            return string.IsNullOrWhiteSpace(reason)
                ? LinkLoginRejectionReasons.TokenInvalid
                : reason;
        }
        catch (Exception)
        {
            return LinkLoginRejectionReasons.TokenInvalid;
        }
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
        returnUrl = NormalizeReturnUrl(returnUrl);
        await SetTenantCookieAsync(tenantId, tenantName);
        return LocalRedirect(returnUrl);
    }

    protected virtual string NormalizeReturnUrl(string? returnUrl)
    {
        returnUrl ??= DefaultAuthenticatedReturnPath;
        if (!Url.IsLocalUrl(returnUrl))
        {
            return DefaultAuthenticatedReturnPath;
        }

        if (IsRootPath(returnUrl))
        {
            return DefaultAuthenticatedReturnPath;
        }

        return returnUrl;
    }

    protected virtual bool IsRootPath(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        return url is "/" or "~/" or "~/";
    }

    protected virtual async Task SetTenantCookieAsync(Guid? tenantId, string? tenantName)
    {
        var cookieName = _tenantOptions.TenantCookieName;
        if (string.IsNullOrEmpty(cookieName))
        {
            return;
        }

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

    private async Task<IActionResult> RedirectToPhoneConfirmationAsync(IdentityUser user)
    {
        var sessions = LazyServiceProvider.LazyGetRequiredService<IPhoneConfirmationSessionStore>();
        if (!sessions.IsSupported)
        {
            return Redirect("/account/login?error=PhoneConfirmationSessionInvalid");
        }

        var sessionToken = await sessions.CreateAsync(user.Id);
        return Redirect("/account/confirm-phone?token=" + Uri.EscapeDataString(sessionToken));
    }
}
