using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Permissions;
using SufiChain.SufiAbp.ShortLinkGenerator.Settings;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[RequiresFeature(SufiAbpShortLinkGeneratorFeatures.Enable, SufiAbpShortLinkGeneratorFeatures.ShortLinks)]
[Authorize(ShortLinkGeneratorPermissions.ShortLinks.Default)]
public class ShortUrlAppService : ApplicationService, IShortUrlAppService
{
    private readonly IShortUrlRepository _repository;
    private readonly IRepository<ShortUrlClick, Guid> _clickRepository;
    private readonly ShortUrlManager _manager;
    private readonly IDistributedCache<ShortUrlCacheItem> _cache;
    private readonly ShortLinkGeneratorOptions _options;
    private readonly ISettingProvider _settingProvider;
    
    public ShortUrlAppService(
        IShortUrlRepository repository,
        IRepository<ShortUrlClick, Guid> clickRepository,
        ShortUrlManager manager,
        IDistributedCache<ShortUrlCacheItem> cache,
        IOptions<ShortLinkGeneratorOptions> options,
        ISettingProvider settingProvider)
    {
        _repository = repository;
        _clickRepository = clickRepository;
        _manager = manager;
        _cache = cache;
        _options = options.Value;
        _settingProvider = settingProvider;
    }
    
    [Authorize(ShortLinkGeneratorPermissions.ShortLinks.Create)]
    public virtual async Task<ShortUrlDto> CreateAsync(CreateShortUrlDto input)
    {
        var shortUrl = await _manager.CreateAsync(
            input.DestinationUrl,
            input.CreatedByModule ?? "Manual",
            input.ExpiresAt,
            input.Description);
            
        await _repository.InsertAsync(shortUrl, autoSave: true);
        await CacheShortUrlAsync(shortUrl);
        
        return await MapToDtoAsync(shortUrl);
    }
    
    public virtual async Task<string> GenerateShortUrlAsync(CreateShortUrlDto input)
    {
        var shortUrl = await CreateAsync(input);
        return shortUrl.FullShortUrl;
    }
    
    public virtual async Task<ShortUrlDto> GetAsync(Guid id)
    {
        var shortUrl = await _repository.GetAsync(id);
        return await MapToDtoAsync(shortUrl);
    }
    
    public virtual async Task<ShortUrlDto> GetByShortCodeAsync(string shortCode)
    {
        // Try cache first
        var cacheKey = GetCacheKey(shortCode);
        var cached = await _cache.GetAsync(cacheKey);
        
        if (cached != null)
        {
            var shortUrl = await _repository.GetAsync(cached.Id);
            return await MapToDtoAsync(shortUrl);
        }
        
        // Fallback to database
        var entity = await _repository.FindByShortCodeAsync(shortCode);
        if (entity == null)
        {
            throw new BusinessException(ShortLinkGeneratorErrorCodes.ShortUrlNotFound)
                .WithData("ShortCode", shortCode);
        }
        
        await CacheShortUrlAsync(entity);
        return await MapToDtoAsync(entity);
    }
    
    public virtual async Task<PagedResultDto<ShortUrlDto>> GetListAsync(GetShortUrlListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        
        query = query.WhereIf(!input.Filter.IsNullOrWhiteSpace(),
            x => x.ShortCode.Contains(input.Filter!) || x.DestinationUrl.Contains(input.Filter!));
        query = query.WhereIf(!input.CreatedByModule.IsNullOrWhiteSpace(),
            x => x.CreatedByModule == input.CreatedByModule!);
        query = query.WhereIf(input.IsActive.HasValue,
            x => x.IsActive == input.IsActive.Value);
        query = query.WhereIf(input.CreatedAfter.HasValue,
            x => x.CreationTime >= input.CreatedAfter.Value);
        query = query.WhereIf(input.CreatedBefore.HasValue,
            x => x.CreationTime <= input.CreatedBefore.Value);
        
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount));
        
        var dtos = new List<ShortUrlDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToDtoAsync(item));
        }
        
        return new PagedResultDto<ShortUrlDto>(totalCount, dtos);
    }
    
    [Authorize(ShortLinkGeneratorPermissions.ShortLinks.Edit)]
    public virtual async Task<ShortUrlDto> UpdateAsync(Guid id, UpdateShortUrlDto input)
    {
        var shortUrl = await _repository.GetAsync(id);
        
        shortUrl.DestinationUrl = input.DestinationUrl;
        shortUrl.ExpiresAt = input.ExpiresAt;
        shortUrl.Description = input.Description;
        shortUrl.IsActive = input.IsActive;
        
        await _repository.UpdateAsync(shortUrl, autoSave: true);
        
        // Update cache
        await CacheShortUrlAsync(shortUrl);
        
        return await MapToDtoAsync(shortUrl);
    }
    
    [Authorize(ShortLinkGeneratorPermissions.ShortLinks.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var shortUrl = await _repository.GetAsync(id);
        
        // Remove from cache
        var cacheKey = GetCacheKey(shortUrl.ShortCode);
        await _cache.RemoveAsync(cacheKey);
        
        await _repository.DeleteAsync(id);
    }
    
    [RequiresFeature(SufiAbpShortLinkGeneratorFeatures.Analytics)]
    [Authorize(ShortLinkGeneratorPermissions.ShortLinks.ViewAnalytics)]
    public virtual async Task<ShortUrlAnalyticsDto> GetAnalyticsAsync(Guid id)
    {
        var shortUrl = await _repository.GetAsync(id);
        
        var query = await _clickRepository.GetQueryableAsync();
        var recentClicks = await AsyncExecuter.ToListAsync(
            query.Where(x => x.ShortUrlId == id)
                .OrderByDescending(x => x.ClickedAt)
                .Take(100));
        
        return new ShortUrlAnalyticsDto
        {
            Id = shortUrl.Id,
            ShortCode = shortUrl.ShortCode,
            ClickCount = shortUrl.ClickCount,
            LastAccessedAt = shortUrl.LastAccessedAt,
            RecentClicks = ObjectMapper.Map<List<ShortUrlClick>, List<ShortUrlClickDto>>(recentClicks)
        };
    }
    
    private async Task CacheShortUrlAsync(ShortUrl shortUrl)
    {
        var cacheKey = GetCacheKey(shortUrl.ShortCode);
            
        await _cache.SetAsync(
            cacheKey,
            new ShortUrlCacheItem
            {
                Id = shortUrl.Id,
                DestinationUrl = shortUrl.DestinationUrl,
                ExpiresAt = shortUrl.ExpiresAt,
                IsActive = shortUrl.IsActive
            },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(await GetCacheExpirationMinutesAsync())
            });
    }
    
    private string GetCacheKey(string shortCode) => $"ShortUrl:Code:{shortCode}";
    
    private async Task<int> GetCacheExpirationMinutesAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.ShortUrl.CacheExpirationMinutes);
        if (int.TryParse(value, out var minutes) && minutes > 0)
        {
            return minutes;
        }
        
        return _options.CacheExpirationMinutes;
    }
    
    private async Task<string> GetBaseUrlAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.BaseUrl);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        
        return _options.BaseUrl;
    }
    
    private async Task<string> GetRedirectRouteAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.ShortUrl.RedirectRoute);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        
        return _options.RedirectRoute;
    }
    
    private async Task<ShortUrlDto> MapToDtoAsync(ShortUrl shortUrl)
    {
        var dto = ObjectMapper.Map<ShortUrl, ShortUrlDto>(shortUrl);
        
        var baseUrl = await GetBaseUrlAsync();
        var redirectRoute = await GetRedirectRouteAsync();
        
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            var normalizedBase = baseUrl.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(redirectRoute))
            {
                dto.FullShortUrl = $"{normalizedBase}/{shortUrl.ShortCode}";
            }
            else
            {
                var normalizedRoute = redirectRoute.Trim('/');
                dto.FullShortUrl = $"{normalizedBase}/{normalizedRoute}/{shortUrl.ShortCode}";
            }
        }
        else
        {
            dto.FullShortUrl = shortUrl.ShortCode;
        }
        
        return dto;
    }
}

