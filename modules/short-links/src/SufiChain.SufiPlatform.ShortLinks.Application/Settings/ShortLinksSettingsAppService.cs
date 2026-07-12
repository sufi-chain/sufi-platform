using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.ShortLinks.Permissions;
using SufiChain.SufiPlatform.ShortLinks.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Manages the short-link generator settings that are editable from the settings UI.
/// </summary>
[Authorize(ShortLinksPermissions.ShortLinks.Edit)]
public class ShortLinksSettingsAppService : ShortLinksAppService, IShortLinksSettingsAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly ShortLinksOptions _options;

    public ShortLinksSettingsAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        IOptions<ShortLinksOptions> options)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _options = options.Value;
    }

    public virtual async Task<ShortLinksSettingsDto> GetAsync()
    {
        return new ShortLinksSettingsDto
        {
            BaseUrl = await GetBaseUrlAsync(),
            RedirectRoute = await GetRedirectRouteAsync(),
            ShortCodeLength = await GetShortCodeLengthAsync(),
            CacheExpirationMinutes = await GetCacheExpirationMinutesAsync(),
            DefaultExpirationDays = await GetDefaultExpirationDaysAsync()
        };
    }

    public virtual async Task UpdateAsync(ShortLinksSettingsDto input)
    {
        var baseUrl = input.BaseUrl?.Trim();

        await _settingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ShortLinksSettings.BaseUrl,
            string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl);

        await _settingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ShortLinksSettings.ShortUrl.RedirectRoute,
            ShortLinkRedirectHelper.NormalizeBaseKey(input.RedirectRoute));

        await _settingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ShortLinksSettings.ShortUrl.ShortCodeLength,
            input.ShortCodeLength.ToString(CultureInfo.InvariantCulture));

        await _settingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ShortLinksSettings.ShortUrl.CacheExpirationMinutes,
            input.CacheExpirationMinutes.ToString(CultureInfo.InvariantCulture));

        await _settingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ShortLinksSettings.ShortUrl.DefaultExpirationDays,
            input.DefaultExpirationDays.ToString(CultureInfo.InvariantCulture));
    }

    private async Task<string> GetBaseUrlAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(ShortLinksSettings.BaseUrl);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        return _options.BaseUrl;
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

    private async Task<int> GetShortCodeLengthAsync()
    {
        return await GetPositiveIntSettingAsync(
            ShortLinksSettings.ShortUrl.ShortCodeLength,
            GetDefaultShortCodeLength(),
            ShortLinksConsts.ShortUrl.MaxShortCodeLength);
    }

    private async Task<int> GetCacheExpirationMinutesAsync()
    {
        return await GetPositiveIntSettingAsync(
            ShortLinksSettings.ShortUrl.CacheExpirationMinutes,
            GetDefaultCacheExpirationMinutes());
    }

    private async Task<int> GetDefaultExpirationDaysAsync()
    {
        return await GetPositiveIntSettingAsync(
            ShortLinksSettings.ShortUrl.DefaultExpirationDays,
            GetDefaultExpirationDays());
    }

    private async Task<int> GetPositiveIntSettingAsync(string name, int fallbackValue, int? maxValue = null)
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
            && parsedValue > 0
            && (!maxValue.HasValue || parsedValue <= maxValue.Value))
        {
            return parsedValue;
        }

        return fallbackValue;
    }

    private int GetDefaultShortCodeLength()
    {
        var configuredValue = _options.ShortCodeLength;
        if (configuredValue > 0 && configuredValue <= ShortLinksConsts.ShortUrl.MaxShortCodeLength)
        {
            return configuredValue;
        }

        return ShortLinksConsts.DefaultShortCodeLength;
    }

    private int GetDefaultCacheExpirationMinutes()
    {
        return _options.CacheExpirationMinutes > 0
            ? _options.CacheExpirationMinutes
            : ShortLinksConsts.DefaultCacheExpirationMinutes;
    }

    private int GetDefaultExpirationDays()
    {
        return _options.DefaultExpirationDays is > 0
            ? _options.DefaultExpirationDays.Value
            : ShortLinksConsts.DefaultExpirationDays;
    }

}
