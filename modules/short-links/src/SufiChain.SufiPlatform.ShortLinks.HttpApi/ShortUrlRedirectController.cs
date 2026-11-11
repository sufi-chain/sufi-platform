using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.Features;
using SufiChain.SufiPlatform.ShortLinks.Settings;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.ShortLinks;

[ApiController]
[AllowAnonymous]
[Area(ShortLinksRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ShortLinksRemoteServiceConsts.RemoteServiceName)]
[Route("api/short-link/redirect")]
public class ShortUrlRedirectController : SufiControllerBase
{
    private readonly IShortUrlRepository _repository;
    private readonly IDistributedCache<ShortUrlCacheItem> _cache;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IFeatureChecker _featureChecker;
    private readonly ISettingProvider _settingProvider;
    private readonly ShortLinksOptions _options;
    
    public ShortUrlRedirectController(
        IShortUrlRepository repository,
        IDistributedCache<ShortUrlCacheItem> cache,
        IDistributedEventBus distributedEventBus,
        IFeatureChecker featureChecker,
        ISettingProvider settingProvider,
        IOptions<ShortLinksOptions> options)
    {
        _repository = repository;
        _cache = cache;
        _distributedEventBus = distributedEventBus;
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

        // 1. Try cache first
        var cacheKey = $"ShortUrl:Code:{shortCode}";
        var cached = await _cache.GetAsync(cacheKey);
        
        ShortUrl? shortUrl = null;
        
        if (cached != null)
        {
            if (!IsValid(cached.ExpiresAt, cached.IsActive))
                return RedirectToNotFound(baseKey, shortCode);
                
            await TrackClickAsync(cached.Id, shortCode, incomingToken);

            return Redirect(ShortLinkRedirectHelper.AppendToken(cached.DestinationUrl, incomingToken));
        }
        
        // 2. Fallback to database
        shortUrl = await _repository.FindByShortCodeAsync(shortCode);
        
        if (shortUrl == null || !IsValid(shortUrl.ExpiresAt, shortUrl.IsActive))
            return RedirectToNotFound(baseKey, shortCode);
        
        // Cache for future requests
        await _cache.SetAsync(cacheKey, new ShortUrlCacheItem
        {
            Id = shortUrl.Id,
            DestinationUrl = shortUrl.DestinationUrl,
            ExpiresAt = shortUrl.ExpiresAt,
            IsActive = shortUrl.IsActive
        });
        
        await TrackClickAsync(shortUrl.Id, shortCode, incomingToken);

        return Redirect(ShortLinkRedirectHelper.AppendToken(shortUrl.DestinationUrl, incomingToken));
    }

    private RedirectResult RedirectToNotFound(string baseKey, string shortCode)
    {
        var original = "/" + baseKey + "/" + shortCode;
        return Redirect("/404?from=" + Uri.EscapeDataString(original));
    }
    
    private bool IsValid(DateTime? expiresAt, bool isActive)
    {
        if (!isActive) return false;
        if (expiresAt.HasValue && expiresAt < Clock.Now) return false;
        return true;
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

    private async Task TrackClickAsync(Guid shortUrlId, string shortCode, string? token)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiShortLinksFeatures.Analytics))
        {
            return;
        }

        try
        {
            // The redirect does not decide who counts. Owner and authenticated visits
            // are clicks. The outbox handler increments ClickCount in the link tenant.
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var dedupSource = !string.IsNullOrWhiteSpace(token) ? token : ipAddress;
            await _distributedEventBus.PublishAsync(new ShortUrlClickedEto
            {
                ShortUrlId = shortUrlId,
                TenantId = CurrentTenant.Id,
                ClickedAt = Clock.Now,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = ipAddress,
                Referrer = Request.Headers["Referer"].ToString(),
                Token = token,
                DedupKey = !string.IsNullOrWhiteSpace(dedupSource) ? $"{shortUrlId}:{dedupSource}" : null
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"Failed to track click for short URL: {shortCode}");
        }
    }
}