using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

public partial class ExternalAuthSettingsGroup : SettingsComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<ExternalAuthSettingsDto> _edits;

    public ExternalAuthSettingsGroup()
    {
        _edits = new SettingGroupEditor<ExternalAuthSettingsDto>(() => _settings, value => _settings = value ?? new ExternalAuthSettingsDto());
    }

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    private IExternalAuthSettingsAppService ExternalAuthSettingsAppService =>
        LazyGetRequiredService(ref _externalAuthSettingsAppService);

    private IExternalAuthSettingsAppService? _externalAuthSettingsAppService;

    private ExternalAuthSettingsDto _settings = new();

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
        _settings = await ExternalAuthSettingsAppService.GetAsync();
        _edits.Capture();
    }, LoadingKeys.Load);

    public async Task SaveAsync()
    {
        if (await TrySaveAsync())
        {
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }
    }

    public async Task<bool> TrySaveAsync()
    {
        var saved = await ExecuteWithLoadingAsync(async () =>
        {
            await ExternalAuthSettingsAppService.UpdateAsync(new UpdateExternalAuthSettingsDto
            {
                Google = _settings.Google,
                Microsoft = _settings.Microsoft,
                GitHub = _settings.GitHub
            });

            _settings.Google.ClientSecret = null;
            _settings.Microsoft.ClientSecret = null;
            _settings.GitHub.ClientSecret = null;
            _edits.Capture();
            return true;
        }, LoadingKeys.Save);

        return saved == true;
    }

    public Task DiscardAsync()
    {
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }
}
