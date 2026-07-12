using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Settings.Localization;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor.Settings;

public partial class ShortLinksSettingsGroup : ShortLinksComponentBase, ISaveableSettingGroup
{
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IShortLinksSettingsAppService SettingsAppService { get; set; } = default!;

    [Inject] private IStringLocalizer<SufiSettingsResource> SettingsLocalizer { get; set; } = default!;

    private ShortLinksSettingsDto _settings = new();

    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

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
    }, LoadingKeys.Load);

    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await SettingsAppService.UpdateAsync(_settings);
        _settings = await SettingsAppService.GetAsync();
        await Notify.SuccessAsync(SettingsLocalizer["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);
}
