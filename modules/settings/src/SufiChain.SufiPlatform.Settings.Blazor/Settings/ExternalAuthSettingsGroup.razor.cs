namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

public partial class ExternalAuthSettingsGroup : SettingsComponentBase, ISaveableSettingGroup
{
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
    }, LoadingKeys.Load);

    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
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
        await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);
}
