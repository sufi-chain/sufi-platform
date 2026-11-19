using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.Features;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Resolves a public short code in the current tenant database and counts the click.
/// Tenants with their own connection string are read through the current tenant.
/// A shared-database tenant is isolated by the tenant filter.
/// </summary>
public class ShortUrlPublicRedirector : DomainService, IShortUrlPublicRedirector, ITransientDependency
{
    private readonly IShortUrlRepository _repository;
    private readonly ShortUrlClickedHandler _clicks;
    private readonly IDistributedCache<ShortUrlCacheItem> _cache;
    private readonly IFeatureChecker _featureChecker;
    private readonly ICurrentTenant _currentTenant;

    public ShortUrlPublicRedirector(
        IShortUrlRepository repository,
        ShortUrlClickedHandler clicks,
        IDistributedCache<ShortUrlCacheItem> cache,
        IFeatureChecker featureChecker,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _clicks = clicks;
        _cache = cache;
        _featureChecker = featureChecker;
        _currentTenant = currentTenant;
    }

    public virtual async Task<ShortUrlPublicRedirect?> TryRedirectAsync(
        string? shortCode,
        string? token,
        string? userAgent,
        string? ipAddress,
        string? referrer)
    {
        if (!IsShortCode(shortCode))
        {
            return null;
        }

        if (!await _featureChecker.IsEnabledAsync(SufiShortLinksFeatures.Enable) ||
            !await _featureChecker.IsEnabledAsync(SufiShortLinksFeatures.PublicRedirect))
        {
            return null;
        }

        var code = shortCode!.Trim();
        var cached = await _cache.GetAsync(CacheKey(code));
        if (cached != null)
        {
            if (!IsValid(cached.ExpiresAt, cached.IsActive))
            {
                return new ShortUrlPublicRedirect { KnownCode = true };
            }

            await TrackAsync(cached.Id, code, token, userAgent, ipAddress, referrer);
            return new ShortUrlPublicRedirect
            {
                KnownCode = true,
                Destination = ShortLinkRedirectHelper.AppendToken(cached.DestinationUrl, token)
            };
        }

        var shortUrl = await _repository.FindByShortCodeAsync(code);
        if (shortUrl == null)
        {
            return null;
        }

        await _cache.SetAsync(
            CacheKey(code),
            new ShortUrlCacheItem
            {
                Id = shortUrl.Id,
                TenantId = shortUrl.TenantId,
                DestinationUrl = shortUrl.DestinationUrl,
                ExpiresAt = shortUrl.ExpiresAt,
                IsActive = shortUrl.IsActive
            },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ShortLinksConsts.DefaultCacheExpirationMinutes)
            });

        if (!IsValid(shortUrl.ExpiresAt, shortUrl.IsActive))
        {
            return new ShortUrlPublicRedirect { KnownCode = true };
        }

        await TrackAsync(shortUrl.Id, code, token, userAgent, ipAddress, referrer);
        return new ShortUrlPublicRedirect
        {
            KnownCode = true,
            Destination = ShortLinkRedirectHelper.AppendToken(shortUrl.DestinationUrl, token)
        };
    }

    public static bool IsShortCode(string? shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return false;
        }

        var code = shortCode.Trim();
        if (code.Length > ShortLinksConsts.ShortUrl.MaxShortCodeLength || code.Contains('.'))
        {
            return false;
        }

        foreach (var character in code)
        {
            if (!char.IsLetterOrDigit(character) && character is not ('-' or '_'))
            {
                return false;
            }
        }

        return true;
    }

    private async Task TrackAsync(
        Guid shortUrlId,
        string shortCode,
        string? token,
        string? userAgent,
        string? ipAddress,
        string? referrer)
    {
        var dedupSource = !string.IsNullOrWhiteSpace(token) ? token : ipAddress;
        await _clicks.HandleEventAsync(new ShortUrlClickedEto
        {
            ShortUrlId = shortUrlId,
            TenantId = _currentTenant.Id,
            ClickedAt = Clock.Now,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            Referrer = referrer,
            Token = token,
            DedupKey = !string.IsNullOrWhiteSpace(dedupSource) ? $"{shortUrlId}:{dedupSource}" : null
        });
    }

    private bool IsValid(DateTime? expiresAt, bool isActive)
    {
        if (!isActive)
        {
            return false;
        }

        return !expiresAt.HasValue || expiresAt.Value >= Clock.Now;
    }

    private string CacheKey(string shortCode) =>
        "ShortUrl:Public:" + (_currentTenant.Id?.ToString("N") ?? "host") + ":" + shortCode;
}
