using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.Features;
using SufiChain.SufiPlatform.ShortLinks.Settings;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.ShortLinks;

[ApiController]
[AllowAnonymous]
[Area(ShortLinksRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ShortLinksRemoteServiceConsts.RemoteServiceName)]
[Route("api/short-link/redirect")]
public class ShortUrlRedirectController : SufiControllerBase
{
    private readonly IShortUrlPublicRedirector _redirector;
    private readonly IFeatureChecker _featureChecker;
    private readonly ISettingProvider _settingProvider;
    private readonly ShortLinksOptions _options;
    
    public ShortUrlRedirectController(
        IShortUrlPublicRedirector redirector,
        IFeatureChecker featureChecker,
        ISettingProvider settingProvider,
        IOptions<ShortLinksOptions> options)
    {
        _redirector = redirector;
        _featureChecker = featureChecker;
        _settingProvider = settingProvider;
        _options = options.Value;
    }
    
    [HttpGet("{baseKey}/{shortCode}")]
    public async Task<IActionResult> RedirectToUrl(string baseKey, string shortCode)
    {
        if (!await IsPublicRedirectEnabledAsync())
        {
            return RedirectToNotFound(baseKey, shortCode);
        }

        if (!await IsValidBaseKeyAsync(baseKey))
        {
            return RedirectToNotFound(baseKey, shortCode);
        }

        var incomingToken = Request.Query.TryGetValue("c", out var c) ? c.ToString() : null;
        var decision = await _redirector.TryRedirectAsync(
            shortCode,
            incomingToken,
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.Referer.ToString());
        if (string.IsNullOrWhiteSpace(decision?.Destination))
        {
            return RedirectToNotFound(baseKey, shortCode);
        }

        if (!ShortLinkRedirectHelper.IsAbsoluteHttpUrl(decision.Destination))
        {
            return NotFound();
        }

        return Redirect(decision.Destination);
    }

    private RedirectResult RedirectToNotFound(string baseKey, string shortCode)
    {
        var original = "/" + baseKey + "/" + shortCode;
        return Redirect("/404?from=" + Uri.EscapeDataString(original));
    }
    
    private async Task<bool> IsPublicRedirectEnabledAsync()
    {
        return await _featureChecker.IsEnabledAsync(SufiShortLinksFeatures.Enable) &&
               await _featureChecker.IsEnabledAsync(SufiShortLinksFeatures.PublicRedirect);
    }

    private async Task<bool> IsValidBaseKeyAsync(string baseKey)
    {
        return string.Equals(
            ShortLinkRedirectHelper.NormalizeBaseKey(baseKey),
            await GetRedirectRouteAsync(),
            StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GetRedirectRouteAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(ShortLinksSettings.ShortUrl.RedirectRoute);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return ShortLinkRedirectHelper.NormalizeBaseKey(value);
        }

        return ShortLinkRedirectHelper.NormalizeBaseKey(_options.RedirectRoute);
    }
}