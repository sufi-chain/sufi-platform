using System.Text.Json;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Storage;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileStructures;

public class FileStructureStorageConfigSerializationTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Output_omits_storage_secrets_and_keeps_presence_flags()
    {
        var config = new FileStructureStorageConfigDto
        {
            StorageProvider = FileStructureStorageProvider.MinIO,
            DatabaseConnectionString = "Server=secret",
            MinioAccessKey = "minio-key",
            MinioSecretKey = "minio-secret",
            S3AccessKeyId = "s3-key",
            S3SecretAccessKey = "s3-secret",
            MinioEndPoint = "https://minio.example",
            HasDatabaseConnectionString = true,
            HasMinioAccessKey = true,
            HasMinioSecretKey = true,
            HasS3AccessKey = true,
            HasS3SecretKey = true
        };

        var json = JsonSerializer.Serialize(config, Json);

        json.ShouldNotContain("\"databaseConnectionString\"");
        json.ShouldNotContain("\"minioAccessKey\"");
        json.ShouldNotContain("\"minioSecretKey\"");
        json.ShouldNotContain("\"s3AccessKeyId\"");
        json.ShouldNotContain("\"s3SecretAccessKey\"");
        json.ShouldNotContain("Server=secret");
        json.ShouldNotContain("minio-secret");
        json.ShouldContain("hasDatabaseConnectionString");
        json.ShouldContain("hasMinioSecretKey");
        json.ShouldContain("hasS3SecretKey");
        json.ShouldContain("https://minio.example");
    }

    [Fact]
    public void Input_still_accepts_storage_secrets()
    {
        const string json = """
            {
              "storageProvider": 2,
              "databaseConnectionString": "Server=write",
              "minioAccessKey": "key",
              "minioSecretKey": "secret",
              "s3AccessKeyId": "ak",
              "s3SecretAccessKey": "sk",
              "hasDatabaseConnectionString": true
            }
            """;

        var config = JsonSerializer.Deserialize<FileStructureStorageConfigDto>(json, Json);

        config.ShouldNotBeNull();
        config.DatabaseConnectionString.ShouldBe("Server=write");
        config.MinioAccessKey.ShouldBe("key");
        config.MinioSecretKey.ShouldBe("secret");
        config.S3AccessKeyId.ShouldBe("ak");
        config.S3SecretAccessKey.ShouldBe("sk");
        config.HasDatabaseConnectionString.ShouldBeTrue();
    }
}
