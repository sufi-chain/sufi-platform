using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerStorageSettingsGroup : FileManagerComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<FileStructureStorageConfigDto> _edits;

    public FileManagerStorageSettingsGroup()
    {
        _edits = new SettingGroupEditor<FileStructureStorageConfigDto>(() => _config, value => _config = value ?? new FileStructureStorageConfigDto());
    }
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
        public const string Test = "test";
    }

    [Inject] private IFileManagerStorageSettingsAppService StorageSettingsAppService { get; set; } = default!;

    private static readonly FileStructureStorageProvider[] _providerOptions =
    {
        FileStructureStorageProvider.Database,
        FileStructureStorageProvider.FileSystem,
        FileStructureStorageProvider.MinIO,
        FileStructureStorageProvider.S3Provider
    };

    private FileStructureStorageConfigDto _config = new();

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
        _config = await StorageSettingsAppService.GetDefaultConfigAsync();
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
            await StorageSettingsAppService.UpdateDefaultConfigAsync(_config);
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

    private Task TestConnectionAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var input = new TestStorageConnectionInput
        {
            StorageProvider = _config.StorageProvider,
            DatabaseConnectionString = _config.DatabaseConnectionString,
            FileSystemBasePath = _config.FileSystemBasePath,
            MinioEndPoint = _config.MinioEndPoint,
            MinioAccessKey = _config.MinioAccessKey,
            MinioSecretKey = _config.MinioSecretKey,
            MinioBucketName = _config.MinioBucketName,
            S3EndPoint = _config.S3EndPoint,
            S3Region = _config.S3Region ?? "us-east-1",
            S3AccessKeyId = _config.S3AccessKeyId,
            S3SecretAccessKey = _config.S3SecretAccessKey,
            S3ContainerName = _config.S3ContainerName
        };
        var result = await StorageSettingsAppService.TestConnectionAsync(input);
        if (result.Success)
            await Notify.SuccessAsync(result.Message);
        else
            await Notify.ErrorAsync(result.Message);
    }, LoadingKeys.Test);

    private string GetProviderLabel(FileStructureStorageProvider p) => p switch
    {
        FileStructureStorageProvider.Database => L["StorageProviderDatabase"],
        FileStructureStorageProvider.FileSystem => L["StorageProviderFileSystem"],
        FileStructureStorageProvider.MinIO => L["StorageProviderMinIO"],
        FileStructureStorageProvider.S3Provider => L["StorageProviderS3"],
        _ => p.ToString()
    };
}
