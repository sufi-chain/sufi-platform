using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Features;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[ApiController]
[AllowAnonymous]
public class ShortUrlRedirectController : SufiAbpControllerBase
{
    private readonly IShortUrlRepository _repository;
    private readonly IDistributedCache<ShortUrlCacheItem> _cache;
    private readonly IRepository<ShortUrlClick, Guid> _clickRepository;
    private readonly IFeatureChecker _featureChecker;
    
    public ShortUrlRedirectController(
        IShortUrlRepository repository,
        IDistributedCache<ShortUrlCacheItem> cache,
        IRepository<ShortUrlClick, Guid> clickRepository,
        IFeatureChecker featureChecker)
    {
        _repository = repository;
        _cache = cache;
        _clickRepository = clickRepository;
        _featureChecker = featureChecker;
    }
    
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectToUrl(string shortCode)
    {
        if (!await IsPublicRedirectEnabledAsync())
        {
            return NotFound();
        }

        // 1. Try cache first
        var cacheKey = $"ShortUrl:Code:{shortCode}";
        var cached = await _cache.GetAsync(cacheKey);
        
        ShortUrl? shortUrl = null;
        
        if (cached != null)
        {
            if (!IsValid(cached.ExpiresAt, cached.IsActive))
                return NotFound();
                
            // Track click asynchronously (fire and forget)
            _ = TrackClickAsync(cached.Id, shortCode);
            
            return Redirect(cached.DestinationUrl);
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
        _ = TrackClickAsync(shortUrl.Id, shortCode);
        
        return Redirect(shortUrl.DestinationUrl);
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

    private async Task TrackClickAsync(Guid shortUrlId, string shortCode)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAbpShortLinkGeneratorFeatures.Analytics))
        {
            return;
        }

        try
        {
            // Increment click count
            await _repository.IncrementClickCountAsync(shortUrlId);
            
            // Record detailed analytics
            var click = new ShortUrlClick(
                GuidGenerator.Create(),
                shortUrlId,
                Clock.Now,
                Request.Headers["User-Agent"].ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["Referer"].ToString());
            
            await _clickRepository.InsertAsync(click, autoSave: true);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"Failed to track click for short URL: {shortCode}");
        }
    }
}
