using SufiChain.SufiAbp.FileManager.Storage;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.FileManager.Settings;

public class FileManagerStorageSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(FileManagerStorageSettingNames.DefaultProvider, "Database", isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.Database.ConnectionString, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.FileSystem.BasePath, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.MinIO.EndPoint, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.MinIO.AccessKey, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.MinIO.SecretKey, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.MinIO.BucketName, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.S3.Endpoint, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.S3.Region, "us-east-1", isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.S3.AccessKeyId, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.S3.SecretAccessKey, null, isVisibleToClients: false, isEncrypted: false),
            new SettingDefinition(FileManagerStorageSettingNames.S3.ContainerName, null, isVisibleToClients: false, isEncrypted: false)
        );
    }
}
