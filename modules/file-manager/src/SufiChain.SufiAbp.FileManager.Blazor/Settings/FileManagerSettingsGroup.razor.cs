using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;
using SufiChain.SufiAbp.FileManager.Permissions;

namespace SufiChain.SufiAbp.FileManager.Blazor.Settings;

public partial class FileManagerSettingsGroup : FileManagerComponentBase, ISaveableSettingGroup
{
    private FileManagerGeneralSettingsGroup? _generalSettingsGroup;
    private FileManagerStorageSettingsGroup? _storageSettingsGroup;
    private FileManagerArchivingSettingsGroup? _archivingSettingsGroup;
    private bool _hasGeneralSettingsPermission;
    private bool _hasStorageSettingsPermission;
    private int _activeTab;

    public bool IsSaving =>
        _generalSettingsGroup?.IsSaving == true ||
        _storageSettingsGroup?.IsSaving == true ||
        _archivingSettingsGroup?.IsSaving == true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _hasGeneralSettingsPermission = await IsGrantedAsync(FileManagerPermissions.Settings.Default);
        _hasStorageSettingsPermission = await IsGrantedAsync(FileManagerPermissions.StorageSettings.Manage);
    }

    public async Task SaveAsync()
    {
        if (_generalSettingsGroup != null)
        {
            await _generalSettingsGroup.SaveAsync();
        }

        if (_storageSettingsGroup != null)
        {
            await _storageSettingsGroup.SaveAsync();
        }

        if (_archivingSettingsGroup != null)
        {
            await _archivingSettingsGroup.SaveAsync();
        }
    }
}
