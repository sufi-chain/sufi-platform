using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerStorageSettingsGroup : FileManagerComponentBase, ISaveableSettingGroup
{
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
    }, LoadingKeys.Load);

    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await StorageSettingsAppService.UpdateDefaultConfigAsync(_config);
        await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);

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
