using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerGeneralSettingsGroup : FileManagerComponentBase, ISaveableSettingGroup
{
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IFileManagerSettingsAppService SettingsAppService { get; set; } = default!;

    private FileManagerGeneralSettingsDto _settings = new();

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
        _settings = await SettingsAppService.GetGeneralSettingsAsync();
    }, LoadingKeys.Load);

    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await SettingsAppService.UpdateGeneralSettingsAsync(_settings);
        await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);
}
