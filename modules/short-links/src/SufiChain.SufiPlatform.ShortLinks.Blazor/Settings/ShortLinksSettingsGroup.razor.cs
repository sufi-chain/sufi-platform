using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Settings.Localization;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor.Settings;

public partial class ShortLinksSettingsGroup : ShortLinksComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<ShortLinksSettingsDto> _edits;

    public ShortLinksSettingsGroup()
    {
        _edits = new SettingGroupEditor<ShortLinksSettingsDto>(() => _settings, value => _settings = value ?? new ShortLinksSettingsDto());
    }
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IShortLinksSettingsAppService SettingsAppService { get; set; } = default!;

    [Inject] private IStringLocalizer<SufiSettingsResource> SettingsLocalizer { get; set; } = default!;

    private ShortLinksSettingsDto _settings = new();

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
            await LoadAsync();
        }
    }

    private Task LoadAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _settings = await SettingsAppService.GetAsync();
        _edits.Capture();
    }, LoadingKeys.Load);

    public async Task SaveAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await PersistAsync();
            await Notify.SuccessAsync(SettingsLocalizer["SettingsSavedSuccessfully"]);
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
        await SettingsAppService.UpdateAsync(_settings);
        _settings = await SettingsAppService.GetAsync();
        _edits.Capture();
    }
}
