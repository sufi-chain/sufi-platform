using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace MyCompanyName.MyProjectName.Blazor.WebSite.Controllers;

/// <summary>
/// Account controller for WebSite host.
/// Handles authentication flows via OpenID Connect.
/// </summary>
public class AccountController : AbpController
{
    [HttpGet]
    public async Task Login(string returnUrl = "/")
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            await HttpContext.ChallengeAsync("oidc", new AuthenticationProperties
            {
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            });
        }
        else
        {
            Response.Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/"
        };
        
        return SignOut(properties, "oidc");
    }
}
