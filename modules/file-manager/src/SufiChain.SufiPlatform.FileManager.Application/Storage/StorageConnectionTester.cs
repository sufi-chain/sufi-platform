using Amazon;
using Amazon.S3;
using Microsoft.Data.SqlClient;
using Minio;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class StorageConnectionTester : ITransientDependency
{

    public virtual async Task<TestStorageConnectionResult> TestAsync(TestStorageConnectionInput input)
    {
        return input.StorageProvider switch
        {
            FileStructureStorageProvider.Database => await TestDatabaseAsync(input),
            FileStructureStorageProvider.FileSystem => await TestFileSystemAsync(input),
            FileStructureStorageProvider.MinIO => await TestMinioAsync(input),
            FileStructureStorageProvider.S3Provider => await TestS3Async(input),
            _ => new TestStorageConnectionResult { Success = false, Message = "Unknown provider" }
        };
    }

    private static async Task<TestStorageConnectionResult> TestDatabaseAsync(TestStorageConnectionInput input)
    {
        var connStr = input.DatabaseConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return new TestStorageConnectionResult { Success = false, Message = "Connection string is required" };
        }

        try
        {
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            await conn.CloseAsync();
            return new TestStorageConnectionResult { Success = true, Message = "Connection successful" };
        }
        catch (Exception ex)
        {
            return new TestStorageConnectionResult { Success = false, Message = ex.Message };
        }
    }

    private static Task<TestStorageConnectionResult> TestFileSystemAsync(TestStorageConnectionInput input)
    {
        var basePath = input.FileSystemBasePath;
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return Task.FromResult(new TestStorageConnectionResult { Success = false, Message = "Base path is required" });
        }

        try
        {
            var fullPath = Path.GetFullPath(basePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            var testFile = Path.Combine(fullPath, ".connection-test-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);

            return Task.FromResult(new TestStorageConnectionResult { Success = true, Message = "Path accessible and writable" });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TestStorageConnectionResult { Success = false, Message = ex.Message });
        }
    }

    private static async Task<TestStorageConnectionResult> TestMinioAsync(TestStorageConnectionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.MinioEndPoint) || string.IsNullOrWhiteSpace(input.MinioAccessKey)
            || string.IsNullOrWhiteSpace(input.MinioSecretKey))
        {
            return new TestStorageConnectionResult { Success = false, Message = "Endpoint, AccessKey and SecretKey are required" };
        }

        try
        {
            var client = new MinioClient()
                .WithEndpoint(input.MinioEndPoint.TrimEnd('/'))
                .WithCredentials(input.MinioAccessKey, input.MinioSecretKey)
                .Build();

            await client.ListBucketsAsync();
            return new TestStorageConnectionResult { Success = true, Message = "Connection successful" };
        }
        catch (Exception ex)
        {
            return new TestStorageConnectionResult { Success = false, Message = ex.Message };
        }
    }

    private static async Task<TestStorageConnectionResult> TestS3Async(TestStorageConnectionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.S3AccessKeyId) || string.IsNullOrWhiteSpace(input.S3SecretAccessKey))
        {
            return new TestStorageConnectionResult { Success = false, Message = "AccessKeyId and SecretAccessKey are required" };
        }

        try
        {
            AmazonS3Config config;
            var region = string.IsNullOrWhiteSpace(input.S3Region) ? "us-east-1" : input.S3Region;

            if (!string.IsNullOrWhiteSpace(input.S3EndPoint))
            {
                config = new AmazonS3Config
                {
                    ServiceURL = input.S3EndPoint.TrimEnd('/'),
                    ForcePathStyle = true,
                    AuthenticationRegion = region
                };
            }
            else
            {
                config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };
            }

            using var client = new AmazonS3Client(input.S3AccessKeyId, input.S3SecretAccessKey, config);
            await client.ListBucketsAsync();
            return new TestStorageConnectionResult { Success = true, Message = "Connection successful" };
        }
        catch (Exception ex)
        {
            return new TestStorageConnectionResult { Success = false, Message = ex.Message };
        }
    }
}
