using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.FileManager.Permissions;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerSettingsGroup : FileManagerComponentBase, IEditableSettingGroup
{
    private readonly HashSet<IEditableSettingGroup> _wired = new();

    private FileManagerGeneralSettingsGroup? _generalSettingsGroup;
    private FileManagerStorageSettingsGroup? _storageSettingsGroup;
    private FileManagerArchivingSettingsGroup? _archivingSettingsGroup;
    private bool _hasGeneralSettingsPermission;
    private bool _hasStorageSettingsPermission;

    public bool IsSaving =>
        _generalSettingsGroup?.IsSaving == true ||
        _storageSettingsGroup?.IsSaving == true ||
        _archivingSettingsGroup?.IsSaving == true;

    public bool HasUnsavedChanges =>
        _generalSettingsGroup?.HasUnsavedChanges == true ||
        _archivingSettingsGroup?.HasUnsavedChanges == true ||
        _storageSettingsGroup?.HasUnsavedChanges == true;

    public event Action? EditStateChanged;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _hasGeneralSettingsPermission = await IsGrantedAsync(FileManagerPermissions.Settings.Default);
        _hasStorageSettingsPermission = await IsGrantedAsync(FileManagerPermissions.StorageSettings.Manage);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        Watch(_generalSettingsGroup);
        Watch(_archivingSettingsGroup);
        Watch(_storageSettingsGroup);
    }

    public async Task SaveAsync()
    {
        if (_generalSettingsGroup != null)
        {
            await _generalSettingsGroup.SaveAsync();
        }

        if (_archivingSettingsGroup != null)
        {
            await _archivingSettingsGroup.SaveAsync();
        }

        if (_storageSettingsGroup != null)
        {
            await _storageSettingsGroup.SaveAsync();
        }
    }

    public async Task<bool> TrySaveAsync()
    {
        var saved = true;
        saved &= await TrySaveChildAsync(_generalSettingsGroup);
        saved &= await TrySaveChildAsync(_archivingSettingsGroup);
        saved &= await TrySaveChildAsync(_storageSettingsGroup);
        return saved;
    }

    public async Task DiscardAsync()
    {
        if (_generalSettingsGroup != null)
        {
            await _generalSettingsGroup.DiscardAsync();
        }

        if (_archivingSettingsGroup != null)
        {
            await _archivingSettingsGroup.DiscardAsync();
        }

        if (_storageSettingsGroup != null)
        {
            await _storageSettingsGroup.DiscardAsync();
        }
    }

    private void Watch(IEditableSettingGroup? editor)
    {
        if (editor == null || !_wired.Add(editor))
        {
            return;
        }

        editor.EditStateChanged += OnChildEditStateChanged;
    }

    private void OnChildEditStateChanged() => EditStateChanged?.Invoke();

    private static async Task<bool> TrySaveChildAsync(IEditableSettingGroup? editor)
    {
        if (editor == null)
        {
            return true;
        }

        return await editor.TrySaveAsync();
    }
}
