namespace SufiChain.SufiPlatform.Settings;

[Microsoft.AspNetCore.Authorization.Authorize(SettingsPermissions.TimeZone)]
public class TimeZoneSettingsAppService : SettingsAppServiceBase, ITimeZoneSettingsAppService
{
    protected ISettingManager SettingManager { get; }

    private const string UnspecifiedTimeZone = "Unspecified";

    public TimeZoneSettingsAppService(ISettingManager settingManager)
    {
        SettingManager = settingManager;
    }

    public virtual async Task<TimeZoneSettingsDto> GetAsync()
    {
        var timezone = CurrentTenant.IsAvailable
            ? await SettingManager.GetOrNullForCurrentTenantAsync(TimingSettingNames.TimeZone)
            : await SettingManager.GetOrNullGlobalAsync(TimingSettingNames.TimeZone);

        return new TimeZoneSettingsDto
        {
            TimeZone = string.IsNullOrWhiteSpace(timezone) ? UnspecifiedTimeZone : timezone
        };
    }

    public virtual Task<List<NameValue>> GetTimezonesAsync()
    {
        var timezones = TimeZoneInfo.GetSystemTimeZones()
            .OrderBy(x => x.DisplayName)
            .Select(x => new NameValue($"{x.Id} ({GetTimezoneOffset(x.BaseUtcOffset)})", x.Id))
            .ToList();

        timezones.Insert(0, new NameValue(L["DefaultTimeZone"], UnspecifiedTimeZone));

        return Task.FromResult(timezones);
    }

    public virtual async Task UpdateAsync(UpdateTimeZoneSettingsDto input)
    {
        var timezone = input.TimeZone;
        if (timezone != null && timezone.Equals(UnspecifiedTimeZone, StringComparison.OrdinalIgnoreCase))
        {
            timezone = null;
        }

        if (CurrentTenant.IsAvailable)
        {
            await SettingManager.SetForCurrentTenantAsync(TimingSettingNames.TimeZone, timezone);
        }
        else
        {
            await SettingManager.SetGlobalAsync(TimingSettingNames.TimeZone, timezone);
        }
    }

    protected virtual string GetTimezoneOffset(TimeSpan offset)
    {
        return offset < TimeSpan.Zero
            ? "-" + offset.ToString(@"hh\:mm")
            : "+" + offset.ToString(@"hh\:mm");
    }
}
