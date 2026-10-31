using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Localization.Settings;

namespace SufiChain.SufiPlatform.Localization.Blazor.Settings;

public partial class LocalizationSettingsGroup : LocalizationComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<LocalizationSettingsDto> _edits;
    private ILocalizationSettingsAppService AppService => LazyGetRequiredService(ref _appService);
    private ILocalizationSettingsAppService? _appService;
    private LocalizationSettingsDto _settings = new();

    public LocalizationSettingsGroup()
    {
        
        _edits = new SettingGroupEditor<LocalizationSettingsDto>(() => _settings, value => _settings = value ?? new LocalizationSettingsDto());
    }

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    public bool HasUnsavedChanges => _edits.HasUnsavedChanges;

    public event Action? EditStateChanged
    {
        add => _edits.Changed += value;
        remove => _edits.Changed -= value;
    }

    protected override void OnInitialized()
    {
        LoadingStates[LoadingKeys.Load] = true;
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
            await LoadAsync();
        }
    }

    private Task LoadAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _settings = await AppService.GetAsync();
        _edits.Capture();
    }, LoadingKeys.Load);

    public async Task SaveAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await PersistAsync();
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }, LoadingKeys.Save);
    }

    public Task<bool> TrySaveAsync() => TrySaveQuietlyAsync(PersistAsync, LoadingKeys.Save);

    public Task DiscardAsync()
    {
        ClearSaveFieldErrors();
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }

    private async Task PersistAsync()
    {
        await AppService.UpdateAsync(_settings);
        _edits.Capture();
    }
}
