using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerArchivingSettingsGroup : FileManagerComponentBase, ISaveableSettingGroup
{
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IFileManagerSettingsAppService SettingsAppService { get; set; } = default!;

    private FileManagerArchivingSettingsDto _settings = new();

    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    private string? AIFilesRetentionDaysText
    {
        get => _settings.AIFilesRetentionDays?.ToString();
        set => _settings.AIFilesRetentionDays = int.TryParse(value, out var retentionDays) ? retentionDays : null;
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
        _settings = await SettingsAppService.GetArchivingSettingsAsync();
    }, LoadingKeys.Load);

    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await SettingsAppService.UpdateArchivingSettingsAsync(_settings);
        await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);
}
