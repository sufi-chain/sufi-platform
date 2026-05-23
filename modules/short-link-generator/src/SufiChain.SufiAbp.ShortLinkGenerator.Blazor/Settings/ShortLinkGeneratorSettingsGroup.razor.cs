using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;
using SufiChain.SufiAbp.SettingManagement.Localization;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Settings;

public partial class ShortLinkGeneratorSettingsGroup : ShortLinkGeneratorComponentBase, ISaveableSettingGroup
{
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IShortLinkGeneratorSettingsAppService SettingsAppService { get; set; } = default!;

    [Inject] private IStringLocalizer<SufiAbpSettingManagementResource> SettingManagementLocalizer { get; set; } = default!;

    private ShortLinkGeneratorSettingsDto _settings = new();

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
        await Notify.SuccessAsync(SettingManagementLocalizer["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);
}
