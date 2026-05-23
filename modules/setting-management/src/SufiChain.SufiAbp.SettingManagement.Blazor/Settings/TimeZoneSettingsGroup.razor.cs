using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.SettingManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using Volo.Abp;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.Blazor.Settings;

/// <summary>
/// Time zone settings group component.
/// Note: This component uses ITimeZoneSettingsAppService (Application Layer) for settings management.
/// Tenant-specific settings should be managed through proper application services, not domain services.
/// </summary>
public partial class TimeZoneSettingsGroup : SettingManagementComponentBase, ISaveableSettingGroup
{

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    private ITimeZoneSettingsAppService TimeZoneSettingsAppService => LazyGetRequiredService(ref _timeZoneSettingsAppService);
    private ITimeZoneSettingsAppService? _timeZoneSettingsAppService;

    private string? _selectedTimeZone;
    private List<NameValue> _timeZones = new();

    /// <summary>
    /// Gets a value indicating whether the save operation is currently in progress.
    /// Implements ISaveableSettingGroup.IsSaving.
    /// </summary>
    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadSettingsAsync();
        }
    }

    private Task LoadSettingsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        // Load available timezones and current setting using Application Service (proper DDD approach)
        _timeZones = await TimeZoneSettingsAppService.GetTimezonesAsync();
        var dto = await TimeZoneSettingsAppService.GetAsync();
        _selectedTimeZone = dto?.TimeZone;
    }, LoadingKeys.Load);

    /// <summary>
    /// Saves the timezone settings.
    /// Implements ISaveableSettingGroup.SaveAsync for centralized save from modal/page footer.
    /// </summary>
    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (!string.IsNullOrEmpty(_selectedTimeZone))
        {
            // Save settings using Application Service (proper DDD approach)
            await TimeZoneSettingsAppService.UpdateAsync(new UpdateTimeZoneSettingsDto { TimeZone = _selectedTimeZone });
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }
    }, LoadingKeys.Save);
    
    private string GetCurrentTimeInSelectedTimeZone()
    {
        if (string.IsNullOrEmpty(_selectedTimeZone))
        {
            return "-";
        }
        
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_selectedTimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return "-";
        }
    }
}
