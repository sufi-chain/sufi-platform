using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Settings;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[ApiController]
[AllowAnonymous]
[Area(ShortLinkGeneratorRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ShortLinkGeneratorRemoteServiceConsts.RemoteServiceName)]
[Route("api/short-link/redirect")]
public class ShortUrlRedirectController : SufiAbpControllerBase
{
    private readonly IShortUrlRepository _repository;
    private readonly IDistributedCache<ShortUrlCacheItem> _cache;
    private readonly IRepository<ShortUrlClick, Guid> _clickRepository;
    private readonly IFeatureChecker _featureChecker;
    private readonly ISettingProvider _settingProvider;
    private readonly ShortLinkGeneratorOptions _options;
    
    public ShortUrlRedirectController(
        IShortUrlRepository repository,
        IDistributedCache<ShortUrlCacheItem> cache,
        IRepository<ShortUrlClick, Guid> clickRepository,
        IFeatureChecker featureChecker,
        ISettingProvider settingProvider,
        IOptions<ShortLinkGeneratorOptions> options)
    {
        _repository = repository;
        _cache = cache;
        _clickRepository = clickRepository;
        _featureChecker = featureChecker;
        _settingProvider = settingProvider;
        _options = options.Value;
    }
    
    [HttpGet("{baseKey}/{shortCode}")]
    public async Task<IActionResult> RedirectToUrl(string baseKey, string shortCode)
    {
        if (!await IsPublicRedirectEnabledAsync())
        {
            return NotFound();
        }

        if (!await IsValidBaseKeyAsync(baseKey))
        {
            return NotFound();
        }

        var incomingToken = Request.Query.TryGetValue("c", out var c) ? c.ToString() : null;

        // 1. Try cache first
        var cacheKey = $"ShortUrl:Code:{shortCode}";
        var cached = await _cache.GetAsync(cacheKey);
        
        ShortUrl? shortUrl = null;
        
        if (cached != null)
        {
            if (!IsValid(cached.ExpiresAt, cached.IsActive))
                return NotFound();
                
            // Track click asynchronously (fire and forget)
            _ = TrackClickAsync(cached.Id, shortCode, incomingToken);

            return Redirect(ShortLinkRedirectHelper.AppendToken(cached.DestinationUrl, incomingToken));
        }
        
        // 2. Fallback to database
        shortUrl = await _repository.FindByShortCodeAsync(shortCode);
        
        if (shortUrl == null || !IsValid(shortUrl.ExpiresAt, shortUrl.IsActive))
            return NotFound();
        
        // Cache for future requests
        await _cache.SetAsync(cacheKey, new ShortUrlCacheItem
        {
            Id = shortUrl.Id,
            DestinationUrl = shortUrl.DestinationUrl,
            ExpiresAt = shortUrl.ExpiresAt,
            IsActive = shortUrl.IsActive
        });
        
        // Track click asynchronously
        _ = TrackClickAsync(shortUrl.Id, shortCode, incomingToken);

        return Redirect(ShortLinkRedirectHelper.AppendToken(shortUrl.DestinationUrl, incomingToken));
    }
    
    private bool IsValid(DateTime? expiresAt, bool isActive)
    {
        if (!isActive) return false;
        if (expiresAt.HasValue && expiresAt < Clock.Now) return false;
        return true;
    }

    private async Task<bool> IsPublicRedirectEnabledAsync()
    {
        return await _featureChecker.IsEnabledAsync(SufiAbpShortLinkGeneratorFeatures.Enable) &&
               await _featureChecker.IsEnabledAsync(SufiAbpShortLinkGeneratorFeatures.PublicRedirect);
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
        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.ShortUrl.RedirectRoute);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return ShortLinkRedirectHelper.NormalizeBaseKey(value);
        }

        return ShortLinkRedirectHelper.NormalizeBaseKey(_options.RedirectRoute);
    }

    private async Task TrackClickAsync(Guid shortUrlId, string shortCode, string? token)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAbpShortLinkGeneratorFeatures.Analytics))
        {
            return;
        }

        try
        {
            // Increment click count
            await _repository.IncrementClickCountAsync(shortUrlId);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var dedupKey = !string.IsNullOrWhiteSpace(token) ? token : ipAddress;

            // Record detailed analytics
            var click = new ShortUrlClick(
                GuidGenerator.Create(),
                shortUrlId,
                Clock.Now,
                Request.Headers["User-Agent"].ToString(),
                ipAddress,
                Request.Headers["Referer"].ToString());

            click.Token = token;
            click.DedupKey = !string.IsNullOrWhiteSpace(dedupKey) ? $"{shortUrlId}:{dedupKey}" : null;

            await _clickRepository.InsertAsync(click, autoSave: true);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"Failed to track click for short URL: {shortCode}");
        }
    }
}
