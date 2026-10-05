using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerArchivingSettingsGroup : FileManagerComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<FileManagerArchivingSettingsDto> _edits;

    public FileManagerArchivingSettingsGroup()
    {
        _edits = new SettingGroupEditor<FileManagerArchivingSettingsDto>(() => _settings, value => _settings = value ?? new FileManagerArchivingSettingsDto());
    }
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IFileManagerSettingsAppService SettingsAppService { get; set; } = default!;

    private FileManagerArchivingSettingsDto _settings = new();

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
        await SettingsAppService.UpdateArchivingSettingsAsync(_settings);
        _edits.Capture();
    }
}
