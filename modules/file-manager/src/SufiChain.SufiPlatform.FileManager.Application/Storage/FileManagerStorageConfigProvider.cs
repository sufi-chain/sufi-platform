using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Reads default storage configuration from settings. No authorization - for internal blob resolution only.
/// </summary>
public class FileManagerStorageConfigProvider : IFileManagerStorageConfigProvider, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }
    protected IStructureStorageConfigEncryption Encryption { get; }

    public FileManagerStorageConfigProvider(
        ISettingProvider settingProvider,
        IStructureStorageConfigEncryption encryption)
    {
        SettingProvider = settingProvider;
        Encryption = encryption;
    }

    public virtual async Task<FileStructureStorageConfigDto> GetDefaultConfigAsync(
        CancellationToken cancellationToken = default)
    {
        var providerStr = await SettingProvider.GetOrNullAsync(FileManagerStorageSettingNames.DefaultProvider)
            ?? FileStructureStorageProvider.Database.ToString();

        if (!Enum.TryParse<FileStructureStorageProvider>(providerStr, ignoreCase: true, out var provider))
        {
            provider = FileStructureStorageProvider.Database;
        }

        return await GetConfigAsync(provider, cancellationToken);
    }

    public virtual async Task<FileStructureStorageConfigDto> GetConfigAsync(
        FileStructureStorageProvider storageProvider,
        CancellationToken cancellationToken = default)
    {
        var config = new FileStructureStorageConfigDto { StorageProvider = storageProvider };

        switch (storageProvider)
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
}
