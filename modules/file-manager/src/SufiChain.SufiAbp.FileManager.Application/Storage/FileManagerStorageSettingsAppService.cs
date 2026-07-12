using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.FileManager.Features;
using SufiChain.SufiAbp.FileManager.Permissions;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.FileManager.Storage;

[RequiresFeature(SufiAbpFileManagerFeatures.Enable, SufiAbpFileManagerFeatures.StorageSettings)]
[Authorize(FileManagerPermissions.StorageSettings.Manage)]
public class FileManagerStorageSettingsAppService : SufiAbpApplicationService, IFileManagerStorageSettingsAppService
{
    protected ISettingProvider SettingProvider { get; }
    protected ISettingManager SettingManager { get; }
    protected IStructureStorageConfigEncryption Encryption { get; }
    protected StorageConnectionTester ConnectionTester { get; }

    public FileManagerStorageSettingsAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        IStructureStorageConfigEncryption encryption,
        StorageConnectionTester connectionTester)
    {
        SettingProvider = settingProvider;
        SettingManager = settingManager;
        Encryption = encryption;
        ConnectionTester = connectionTester;
    }

    public virtual async Task<FileStructureStorageConfigDto> GetDefaultConfigAsync()
    {
        var providerStr = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.DefaultProvider)
            ?? FileStructureStorageProvider.Database.ToString();

        if (!Enum.TryParse<FileStructureStorageProvider>(providerStr, ignoreCase: true, out var provider))
        {
            provider = FileStructureStorageProvider.Database;
        }

        var config = new FileStructureStorageConfigDto { StorageProvider = provider };

        switch (provider)
        {
            case FileStructureStorageProvider.Database:
                var connStr = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.Database.ConnectionString);
                config.HasDatabaseConnectionString = !string.IsNullOrEmpty(connStr);
                config.DatabaseConnectionString = config.HasDatabaseConnectionString ? Encryption.DecryptSensitiveValue(connStr) : null;
                break;

            case FileStructureStorageProvider.FileSystem:
                config.FileSystemBasePath = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.FileSystem.BasePath);
                break;

            case FileStructureStorageProvider.MinIO:
                config.MinioEndPoint = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.MinIO.EndPoint);
                config.MinioBucketName = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.MinIO.BucketName);
                var minioKey = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.MinIO.AccessKey);
                var minioSecret = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.MinIO.SecretKey);
                config.HasMinioAccessKey = !string.IsNullOrEmpty(minioKey);
                config.HasMinioSecretKey = !string.IsNullOrEmpty(minioSecret);
                config.MinioAccessKey = config.HasMinioAccessKey ? Encryption.DecryptSensitiveValue(minioKey) : null;
                config.MinioSecretKey = config.HasMinioSecretKey ? Encryption.DecryptSensitiveValue(minioSecret) : null;
                break;

            case FileStructureStorageProvider.S3Provider:
                config.S3EndPoint = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.S3.Endpoint);
                config.S3Region = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.S3.Region) ?? "us-east-1";
                config.S3ContainerName = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.S3.ContainerName);
                var s3Key = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.S3.AccessKeyId);
                var s3Secret = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.S3.SecretAccessKey);
                config.HasS3AccessKey = !string.IsNullOrEmpty(s3Key);
                config.HasS3SecretKey = !string.IsNullOrEmpty(s3Secret);
                config.S3AccessKeyId = config.HasS3AccessKey ? Encryption.DecryptSensitiveValue(s3Key) : null;
                config.S3SecretAccessKey = config.HasS3SecretKey ? Encryption.DecryptSensitiveValue(s3Secret) : null;
                break;
        }

        return config;
    }

    public virtual async Task UpdateDefaultConfigAsync(FileStructureStorageConfigDto input)
    {
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.DefaultProvider, input.StorageProvider.ToString());

        switch (input.StorageProvider)
        {
            case FileStructureStorageProvider.Database:
                await SetEncryptedAsync(FileManagerStorageSettingNames.Database.ConnectionString, input.DatabaseConnectionString, input.HasDatabaseConnectionString);
                break;

            case FileStructureStorageProvider.FileSystem:
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.FileSystem.BasePath, input.FileSystemBasePath ?? "");
                break;

            case FileStructureStorageProvider.MinIO:
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.MinIO.EndPoint, input.MinioEndPoint ?? "");
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.MinIO.BucketName, input.MinioBucketName ?? "");
                await SetEncryptedAsync(FileManagerStorageSettingNames.MinIO.AccessKey, input.MinioAccessKey, input.HasMinioAccessKey);
                await SetEncryptedAsync(FileManagerStorageSettingNames.MinIO.SecretKey, input.MinioSecretKey, input.HasMinioSecretKey);
                break;

            case FileStructureStorageProvider.S3Provider:
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.S3.Endpoint, input.S3EndPoint ?? "");
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.S3.Region, input.S3Region ?? "us-east-1");
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerStorageSettingNames.S3.ContainerName, input.S3ContainerName ?? "");
                await SetEncryptedAsync(FileManagerStorageSettingNames.S3.AccessKeyId, input.S3AccessKeyId, input.HasS3AccessKey);
                await SetEncryptedAsync(FileManagerStorageSettingNames.S3.SecretAccessKey, input.S3SecretAccessKey, input.HasS3SecretKey);
                break;
        }
    }

    public virtual async Task<TestStorageConnectionResult> TestConnectionAsync(TestStorageConnectionInput input)
    {
        return await ConnectionTester.TestAsync(input);
    }

    private async Task SetEncryptedAsync(string key, string? value, bool hasValue)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var encrypted = Encryption.EncryptSensitiveValue(value);
            if (!string.IsNullOrEmpty(encrypted))
            {
                await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, key, encrypted);
            }
        }
        else if (!hasValue)
        {
            await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, key, null);
        }
    }
}
