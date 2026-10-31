using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Settings;
using Volo.Abp.Validation;

using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Tags;

namespace SufiChain.SufiPlatform.Tags.Settings;

/// <summary>Saves tag limits. The name length cannot exceed the tag column.</summary>
[Authorize(TagsPermissions.Settings.Manage)]
public class TagsSettingsAppService : SufiApplicationService, ITagsSettingsAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;

    public TagsSettingsAppService(ISettingProvider settingProvider, ISettingManager settingManager)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
    }

    public virtual async Task<TagsSettingsDto> GetAsync()
    {
        return new TagsSettingsDto
        {
            MaxTagsPerEntity = await ReadIntAsync(TagsSettings.MaxTagsPerEntity, 10),
            MaxTagNameLength = await ReadIntAsync(TagsSettings.MaxTagNameLength, TagConsts.MaxNameLength)
        };
    }

    public virtual async Task UpdateAsync(TagsSettingsDto input)
    {
        RequireRange(input.MaxTagsPerEntity, 1, 1000, nameof(TagsSettingsDto.MaxTagsPerEntity), "Maximum tags per entity must be between 1 and 1000.");
        RequireRange(input.MaxTagNameLength, 1, TagConsts.MaxNameLength, nameof(TagsSettingsDto.MaxTagNameLength), "Maximum tag name length must be between 1 and 64.");
        await SetAsync(TagsSettings.MaxTagsPerEntity, input.MaxTagsPerEntity.ToString(CultureInfo.InvariantCulture));
        await SetAsync(TagsSettings.MaxTagNameLength, input.MaxTagNameLength.ToString(CultureInfo.InvariantCulture));
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
