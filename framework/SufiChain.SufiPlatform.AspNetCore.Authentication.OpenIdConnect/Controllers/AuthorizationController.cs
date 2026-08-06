using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SufiChain.SufiPlatform.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.AspNetCore.Authentication.OpenIdConnect.Controllers;

/// <summary>
/// OpenIddict authorization controller that redirects to Blazor login pages
/// instead of using MVC Razor Pages. This allows a Blazor-only UI for authentication.
/// </summary>
[Route("connect")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizeController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IdentityUserManager _userManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IdentitySecurityLogManager _securityLogManager;

    public AuthorizeController(
        SignInManager<IdentityUser> signInManager,
        IdentityUserManager userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        IdentitySecurityLogManager securityLogManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _securityLogManager = securityLogManager;
    }

    /// <summary>
    /// Handles the authorization endpoint for OAuth2/OpenID Connect.
    /// If user is not authenticated, redirects to Blazor login page.
    /// </summary>
    [HttpGet("authorize")]
    [HttpPost("authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to authenticate the user using the Identity application scheme
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // Determine if authentication challenge is needed:
        // - User not authenticated
        // - prompt=login was requested (and we haven't already redirected - prevents infinite loop)
        // - max_age=0 or cookie is too old
        var needsChallenge = result is not { Succeeded: true } ||
            ((request.HasPromptValue(PromptValues.Login) ||
              request.MaxAge is 0 ||
              (request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
               DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))) &&
             TempData["IgnoreAuthenticationChallenge"] is null or false);

        if (needsChallenge)
        {
            // If the client requested promptless authentication, return an error
            if (request.HasPromptValue(PromptValues.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));
            }

            // Set flag to prevent infinite redirect loops between login and authorize endpoints.
            // After login redirects back here, this flag ensures we don't challenge again.
            TempData["IgnoreAuthenticationChallenge"] = true;

            // Redirect to Blazor login page - build return URL with original OAuth parameters
            var returnUrl = Request.PathBase + Request.Path + QueryString.Create(
                Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList());

            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties { RedirectUri = returnUrl });
        }

        // Retrieve the application details
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the user profile
        var user = await _userManager.GetUserAsync(result.Principal!) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        // Validate that the user is allowed to sign in (not inactive, not locked out)
        if (!await PreSignInCheckAsync(user))
        {
            // Log security event: user not allowed to sign in
            await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
            {
                Identity = "OpenIddict",
                Action = IdentitySecurityLogActionConsts.LoginNotAllowed,
                UserName = user.UserName,
                ClientId = request.ClientId
            });

            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not allowed to sign in."
                }));
        }

        // Retrieve the permanent authorizations associated with the user/application
        var authorizations = await _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException(),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var consentType = await _applicationManager.GetConsentTypeAsync(application);

        switch (consentType)
        {
            // If the consent is external, immediately return an error if no authorization exists
            case ConsentTypes.External when authorizations.Count == 0:
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application."
                    }));

            // For implicit consent or existing authorization, skip consent form
            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Count != 0:
            case ConsentTypes.Explicit when authorizations.Count != 0 && !request.HasPromptValue(PromptValues.Consent):
                return await CreateAuthorizationResponse(request, user, result.Principal!, application, authorizations);

            // If prompt=none was specified but consent is required, return an error
            case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
            case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required."
                    }));

            // For explicit/systematic consent without existing authorization,
            // since we're Blazor-only and all our apps use implicit consent,
            // we shouldn't reach here. But if we do, just create the authorization.
            default:
                return await CreateAuthorizationResponse(request, user, result.Principal!, application, authorizations);
        }
    }

    private async Task<IActionResult> CreateAuthorizationResponse(
        OpenIddictRequest request,
        IdentityUser user,
        ClaimsPrincipal authenticatedPrincipal,
        object application,
        List<object> authorizations)
    {
        // Create a new ClaimsPrincipal for the user
        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        // Copy the session ID from the authenticated principal if present
        var sid = authenticatedPrincipal.FindFirst(JwtRegisteredClaimNames.Sid);
        if (sid != null)
        {
            var identity = principal.Identities.FirstOrDefault();
            if (identity != null)
            {
                // Remove existing sid claims and add the new one
                var existingSidClaims = identity.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sid).ToList();
                foreach (var claim in existingSidClaims)
                {
                    identity.RemoveClaim(claim);
                }
                identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sid, sid.Value));
            }
        }

        // Set the scopes and resources
        principal.SetScopes(request.GetScopes());

        var resources = new List<string>();
        await foreach (var resource in _scopeManager.ListResourcesAsync(principal.GetScopes()))
        {
            resources.Add(resource);
        }
        principal.SetResources(resources);

        // Create or reuse an authorization
        var authorization = authorizations.LastOrDefault();
        if (authorization == null)
        {
            authorization = await _authorizationManager.CreateAsync(
                principal: principal,
                subject: await _userManager.GetUserIdAsync(user),
                client: await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException(),
                type: AuthorizationTypes.Permanent,
                scopes: principal.GetScopes());
        }

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

        // Log security event: authorization successful
        await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
        {
            Identity = "OpenIddict",
            Action = IdentitySecurityLogActionConsts.LoginSucceeded,
            UserName = user.UserName,
            ClientId = request.ClientId
        });

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Validates that the user is allowed to sign in.
    /// Checks: IsActive, CanSignIn (email/phone confirmation), IsLockedOut.
    /// </summary>
    private async Task<bool> PreSignInCheckAsync(IdentityUser user)
    {
        if (!user.IsActive)
        {
            return false;
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            return false;
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Handles the logout endpoint for OAuth2/OpenID Connect.
    /// </summary>
    [HttpGet("logout")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Get current user info for logging before sign out
        var userName = User.Identity?.Name;

        // Log security event: logout
        await _securityLogManager.SaveAsync(new IdentitySecurityLogContext
        {
            Identity = "OpenIddict",
            Action = IdentitySecurityLogActionConsts.Logout,
            UserName = userName
        });

        // Sign out the user from Identity
        await _signInManager.SignOutAsync();

        // Sign out from OpenIddict
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }
}
