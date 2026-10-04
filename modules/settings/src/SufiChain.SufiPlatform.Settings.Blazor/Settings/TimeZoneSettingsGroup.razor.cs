using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using Volo.Abp;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Time zone settings group component.
/// Note: This component uses ITimeZoneSettingsAppService (Application Layer) for settings management.
/// Tenant-specific settings should be managed through proper application services, not domain services.
/// </summary>
public partial class TimeZoneSettingsGroup : SettingsComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<TimeZoneEdit> _edits;

    public TimeZoneSettingsGroup()
    {
        _edits = new SettingGroupEditor<TimeZoneEdit>(() => new TimeZoneEdit(_selectedTimeZone), value => _selectedTimeZone = value?.TimeZone);
    }

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    private ITimeZoneSettingsAppService TimeZoneSettingsAppService => LazyGetRequiredService(ref _timeZoneSettingsAppService);
    private ITimeZoneSettingsAppService? _timeZoneSettingsAppService;

    private string? _selectedTimeZone;
    private List<NameValue> _timeZones = new();

    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    public bool HasUnsavedChanges => _edits.HasUnsavedChanges;

    public event Action? EditStateChanged
    {
        add => _edits.Changed += value;
        remove => _edits.Changed -= value;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        _edits.Observe();
    }

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
        _timeZones = await TimeZoneSettingsAppService.GetTimezonesAsync();
        var dto = await TimeZoneSettingsAppService.GetAsync();
        _selectedTimeZone = dto?.TimeZone;
        _edits.Capture();
    }, LoadingKeys.Load);

    public async Task SaveAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            if (!await PersistAsync())
            {
                return;
            }

            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }, LoadingKeys.Save);
    }

    public async Task<bool> TrySaveAsync()
    {
        if (string.IsNullOrEmpty(_selectedTimeZone))
        {
            return false;
        }

        return await TrySaveQuietlyAsync(async () =>
        {
            await TimeZoneSettingsAppService.UpdateAsync(new UpdateTimeZoneSettingsDto { TimeZone = _selectedTimeZone });
            _edits.Capture();
        }, LoadingKeys.Save);
    }

    private async Task<bool> PersistAsync()
    {
        if (string.IsNullOrEmpty(_selectedTimeZone))
        {
            return false;
        }

        await TimeZoneSettingsAppService.UpdateAsync(new UpdateTimeZoneSettingsDto { TimeZone = _selectedTimeZone });
        _edits.Capture();
        return true;
    }

    public Task DiscardAsync()
    {
        ClearSaveFieldErrors();
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }

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

    private sealed class TimeZoneEdit
    {
        public TimeZoneEdit()
        {
        }

        public TimeZoneEdit(string? timeZone)
        {
            TimeZone = timeZone;
        }

        public string? TimeZone { get; set; }
    }
}
