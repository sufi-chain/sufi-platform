using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using shortid;
using SufiChain.SufiAbp.ShortLinkGenerator.Settings;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public class ShortUrlManager : DomainService
{
    private readonly IShortUrlRepository _repository;
    private readonly ShortLinkGeneratorOptions _options;
    private readonly ISettingProvider _settingProvider;
    
    public ShortUrlManager(
        IShortUrlRepository repository,
        IOptions<ShortLinkGeneratorOptions> options,
        ISettingProvider settingProvider)
    {
        _repository = repository;
        _options = options.Value;
        _settingProvider = settingProvider;
    }
    
    public virtual async Task<ShortUrl> CreateAsync(
        string destinationUrl,
        string createdByModule,
        DateTime? expiresAt = null,
        string? description = null)
    {
        Check.NotNullOrWhiteSpace(destinationUrl, nameof(destinationUrl));
        Check.NotNullOrWhiteSpace(createdByModule, nameof(createdByModule));
        
        var shortCode = await GenerateUniqueShortCodeAsync();
        var effectiveExpiration = await ResolveExpirationAsync(expiresAt);
        
        return new ShortUrl(
            GuidGenerator.Create(),
            shortCode,
            destinationUrl,
            createdByModule,
            effectiveExpiration,
            description);
    }
    
    protected virtual async Task<string> GenerateUniqueShortCodeAsync()
    {
        var length = await GetShortCodeLengthAsync();
        
        var maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            var options = new ShortIdOptions(
                useNumbers: true,
                useSpecialCharacters: false,
                length: length
            );
            
            var shortCode = ShortId.Generate(options);
            
            if (!await _repository.ShortCodeExistsAsync(shortCode))
            {
                return shortCode;
            }
        }
        
        throw new BusinessException(ShortLinkGeneratorErrorCodes.ShortUrlGenerationFailed)
            .WithData("Attempts", maxAttempts);
    }

    protected virtual async Task<int> GetShortCodeLengthAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.ShortUrl.ShortCodeLength);
        if (int.TryParse(value, out var length) && length > 0 && length <= ShortLinkGeneratorConsts.ShortUrl.MaxShortCodeLength)
        {
            return length;
        }
        
        return _options.ShortCodeLength;
    }
    
    protected virtual async Task<DateTime?> ResolveExpirationAsync(DateTime? requestedExpiration)
    {
        if (requestedExpiration.HasValue)
        {
            return requestedExpiration;
        }
        
        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.ShortUrl.DefaultExpirationDays);
        if (int.TryParse(value, out var defaultDays) && defaultDays > 0)
        {
            return Clock.Now.AddDays(defaultDays);
        }
        
        if (_options.DefaultExpirationDays.HasValue && _options.DefaultExpirationDays.Value > 0)
        {
            return Clock.Now.AddDays(_options.DefaultExpirationDays.Value);
        }
        
        return null;
    }
}

