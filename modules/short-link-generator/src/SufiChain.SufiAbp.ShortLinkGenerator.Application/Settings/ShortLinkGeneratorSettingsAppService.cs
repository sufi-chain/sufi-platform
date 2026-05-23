using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.ShortLinkGenerator.Permissions;
using SufiChain.SufiAbp.ShortLinkGenerator.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// Manages the short-link generator settings that are editable from the settings UI.
/// </summary>
//[Authorize(ShortLinkGeneratorPermissions.ShortLinks.Edit)]
//public class ShortLinkGeneratorSettingsAppService : ShortLinkGeneratorAppService, IShortLinkGeneratorSettingsAppService
//{
//    private readonly ISettingProvider _settingProvider;
//    private readonly ISettingManager _settingManager;
//    private readonly ShortLinkGeneratorOptions _options;

//    public ShortLinkGeneratorSettingsAppService(
//        ISettingProvider settingProvider,
//        ISettingManager settingManager,
//        IOptions<ShortLinkGeneratorOptions> options)
//    {
//        _settingProvider = settingProvider;
//        _settingManager = settingManager;
//        _options = options.Value;
//    }

//    public virtual async Task<ShortLinkGeneratorSettingsDto> GetAsync()
//    {
//        return new ShortLinkGeneratorSettingsDto
//        {
//            BaseUrl = await GetBaseUrlAsync(),
//            ShortCodeLength = await GetShortCodeLengthAsync(),
//            CacheExpirationMinutes = await GetCacheExpirationMinutesAsync(),
//            DefaultExpirationDays = await GetDefaultExpirationDaysAsync()
//        };
//    }

//    public virtual async Task UpdateAsync(ShortLinkGeneratorSettingsDto input)
//    {
//        var baseUrl = input.BaseUrl?.Trim();

//        await _settingManager.SetForTenantOrGlobalAsync(
//            CurrentTenant.Id,
//            ShortLinkGeneratorSettings.BaseUrl,
//            string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl);

//        await _settingManager.SetForTenantOrGlobalAsync(
//            CurrentTenant.Id,
//            ShortLinkGeneratorSettings.ShortUrl.ShortCodeLength,
//            input.ShortCodeLength.ToString(CultureInfo.InvariantCulture));

//        await _settingManager.SetForTenantOrGlobalAsync(
//            CurrentTenant.Id,
//            ShortLinkGeneratorSettings.ShortUrl.CacheExpirationMinutes,
//            input.CacheExpirationMinutes.ToString(CultureInfo.InvariantCulture));

//        await _settingManager.SetForTenantOrGlobalAsync(
//            CurrentTenant.Id,
//            ShortLinkGeneratorSettings.ShortUrl.DefaultExpirationDays,
//            input.DefaultExpirationDays.ToString(CultureInfo.InvariantCulture));
//    }

//    private async Task<string> GetBaseUrlAsync()
//    {
//        var value = await _settingProvider.GetOrNullAsync(ShortLinkGeneratorSettings.BaseUrl);
//        if (!string.IsNullOrWhiteSpace(value))
//        {
//            return value.Trim();
//        }

//        return _options.BaseUrl;
//    }

//    private async Task<int> GetShortCodeLengthAsync()
//    {
//        return await GetPositiveIntSettingAsync(
//            ShortLinkGeneratorSettings.ShortUrl.ShortCodeLength,
//            GetDefaultShortCodeLength(),
//            ShortLinkGeneratorConsts.ShortUrl.MaxShortCodeLength);
//    }

//    private async Task<int> GetCacheExpirationMinutesAsync()
//    {
//        return await GetPositiveIntSettingAsync(
//            ShortLinkGeneratorSettings.ShortUrl.CacheExpirationMinutes,
//            GetDefaultCacheExpirationMinutes());
//    }

//    private async Task<int> GetDefaultExpirationDaysAsync()
//    {
//        return await GetPositiveIntSettingAsync(
//            ShortLinkGeneratorSettings.ShortUrl.DefaultExpirationDays,
//            GetDefaultExpirationDays());
//    }

//    private async Task<int> GetPositiveIntSettingAsync(string name, int fallbackValue, int? maxValue = null)
//    {
//        var value = await _settingProvider.GetOrNullAsync(name);
//        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
//            && parsedValue > 0
//            && (!maxValue.HasValue || parsedValue <= maxValue.Value))
//        {
//            return parsedValue;
//        }

//        return fallbackValue;
//    }

//    private int GetDefaultShortCodeLength()
//    {
//        var configuredValue = _options.ShortCodeLength;
//        if (configuredValue > 0 && configuredValue <= ShortLinkGeneratorConsts.ShortUrl.MaxShortCodeLength)
//        {
//            return configuredValue;
//        }

//        return ShortLinkGeneratorConsts.DefaultShortCodeLength;
//    }

//    private int GetDefaultCacheExpirationMinutes()
//    {
//        return _options.CacheExpirationMinutes > 0
//            ? _options.CacheExpirationMinutes
//            : ShortLinkGeneratorConsts.DefaultCacheExpirationMinutes;
//    }

//    private int GetDefaultExpirationDays()
//    {
//        return _options.DefaultExpirationDays is > 0
//            ? _options.DefaultExpirationDays.Value
//            : ShortLinkGeneratorConsts.DefaultExpirationDays;
//    }
//}
