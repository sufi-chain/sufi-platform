using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Settings;
using Volo.Abp.Validation;

using System.Text.RegularExpressions;
using SufiChain.SufiPlatform.Localization.Permissions;

namespace SufiChain.SufiPlatform.Localization.Settings;

/// <summary>Saves the product default culture. Persian is the definition default.</summary>
[Authorize(LocalizationPermissions.Settings.Manage)]
public class LocalizationSettingsAppService : SufiApplicationService, ILocalizationSettingsAppService
{
    private static readonly Regex CulturePattern = new("^[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})?$", RegexOptions.CultureInvariant);

    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;

    public LocalizationSettingsAppService(ISettingProvider settingProvider, ISettingManager settingManager)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
    }

    public virtual async Task<LocalizationSettingsDto> GetAsync()
    {
        var culture = await _settingProvider.GetOrNullAsync(LocalizationSettingNames.DefaultCulture);
        return new LocalizationSettingsDto
        {
            DefaultCulture = string.IsNullOrWhiteSpace(culture) ? "fa" : culture.Trim()
        };
    }

    public virtual async Task UpdateAsync(LocalizationSettingsDto input)
    {
        var culture = input.DefaultCulture?.Trim() ?? string.Empty;
        if (!CulturePattern.IsMatch(culture))
        {
            throw Invalid(nameof(LocalizationSettingsDto.DefaultCulture), "Default culture must look like fa or fa-IR.");
        }

        await SetAsync(LocalizationSettingNames.DefaultCulture, culture);
    }

    private async Task<int> ReadIntAsync(string name, int fallback)
    {
        var raw = await _settingProvider.GetOrNullAsync(name);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }

    private async Task<bool> ReadBoolAsync(string name, bool fallback)
    {
        var raw = await _settingProvider.GetOrNullAsync(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) || raw == "1";
    }

    private async Task SetAsync(string name, string? value)
    {
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value);
    }

    private static void RequireRange(int value, int min, int max, string member, string message)
    {
        if (value < min || value > max)
        {
            throw Invalid(member, message);
        }
    }

    private static AbpValidationException Invalid(string member, string message)
    {
        return new AbpValidationException(message, new List<ValidationResult>
        {
            new(message, new[] { member })
        });
    }
}
