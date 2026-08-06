namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Setting keys for default file-manager storage configuration.
/// </summary>
public static class FileManagerStorageSettingNames
{
    public const string Prefix = "SufiFileManager.Storage";

    public const string DefaultProvider = Prefix + ".DefaultProvider";

    public static class Database
    {
        public const string ConnectionString = Prefix + ".Database.ConnectionString";
    }

    public static class FileSystem
    {
        public const string BasePath = Prefix + ".FileSystem.BasePath";
    }

    public static class MinIO
    {
        public const string EndPoint = Prefix + ".MinIO.EndPoint";
        public const string AccessKey = Prefix + ".MinIO.AccessKey";
        public const string SecretKey = Prefix + ".MinIO.SecretKey";
        public const string BucketName = Prefix + ".MinIO.BucketName";
    }

    public static class S3
    {
        public const string Endpoint = Prefix + ".S3.Endpoint";
        public const string Region = Prefix + ".S3.Region";
        public const string AccessKeyId = Prefix + ".S3.AccessKeyId";
        public const string SecretAccessKey = Prefix + ".S3.SecretAccessKey";
        public const string ContainerName = Prefix + ".S3.ContainerName";
    }
}