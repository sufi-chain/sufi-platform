using System;
using System.IO;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp.BlobStoring;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.BlobStoring.Minio;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using Volo.Abp.Collections;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.FileManager.Storage;

using Caching = SufiChain.SufiPlatform.FileManager.Caching;

/// <summary>
/// Provides blob container configuration for file-manager containers.
/// For sufi-file-manager and sufi-file-manager-{structureKey}, resolves config from FileStructure ExtraProperties.
/// Uses structure cache when available to avoid database queries.
/// </summary>
public class StructureBlobContainerConfigurationProvider : IBlobContainerConfigurationProvider, ITransientDependency
{
    protected AbpBlobStoringOptions Options { get; }
    protected IFileStructureRepository FileStructureRepository { get; }
    protected IStructureCache StructureCache { get; }
    protected IStructureStorageConfigEncryption Encryption { get; }
    protected IFileManagerStorageConfigProvider StorageConfigProvider { get; }

    public StructureBlobContainerConfigurationProvider(
        IOptions<AbpBlobStoringOptions> options,
        IFileStructureRepository fileStructureRepository,
        IStructureCache structureCache,
        IStructureStorageConfigEncryption encryption,
        IFileManagerStorageConfigProvider storageConfigProvider)
    {
        Options = options.Value;
        FileStructureRepository = fileStructureRepository;
        StructureCache = structureCache;
        Encryption = encryption;
        StorageConfigProvider = storageConfigProvider;
    }

    public BlobContainerConfiguration Get(string name)
    {
        if (!IsFileManagerContainer(name))
        {
            return Options.Containers.GetConfiguration(name);
        }

        var structureKey = ParseStructureKey(name);
        if (string.IsNullOrEmpty(structureKey))
        {
            return Options.Containers.GetConfiguration(FileStructureStorageConstants.DefaultContainerName);
        }

        // Try cache first
        var cached = AsyncHelper.RunSync(() => StructureCache.GetAsync(structureKey));
        if (cached != null)
        {
            return BuildConfigurationFromCacheEntry(cached);
        }

        // Fallback to repository (e.g. cache miss, structure just created)
        var structure = AsyncHelper.RunSync(() => FileStructureRepository.FindByKeyAsync(structureKey));
        if (structure == null)
        {
            return GetDefaultConfigurationFromSettings(structureKey);
        }

        return BuildConfigurationFromStructure(structure);
    }

    private static bool IsFileManagerContainer(string name)
    {
        return name == FileStructureStorageConstants.DefaultContainerName
               || name.StartsWith(FileStructureStorageConstants.ContainerNamePrefix, StringComparison.Ordinal);
    }

    private static string? ParseStructureKey(string name)
    {
        if (name == FileStructureStorageConstants.DefaultContainerName)
        {
            return null;
        }

        return name.StartsWith(FileStructureStorageConstants.ContainerNamePrefix, StringComparison.Ordinal)
            ? name[FileStructureStorageConstants.ContainerNamePrefix.Length..]
            : null;
    }

    private BlobContainerConfiguration BuildConfigurationFromCacheEntry(Caching.StructureCacheEntry entry)
    {
        var providerStr = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.Provider) as string;
        if (string.IsNullOrEmpty(providerStr))
        {
            return GetDefaultConfigurationFromSettings(entry.Key);
        }

        if (!Enum.TryParse<FileStructureStorageProvider>(providerStr, ignoreCase: true, out var provider))
        {
            return GetDefaultConfigurationFromSettings(entry.Key);
        }

        return provider switch
        {
            FileStructureStorageProvider.Database => BuildDatabaseConfigurationFromEntry(entry),
            FileStructureStorageProvider.FileSystem => BuildFileSystemConfigurationFromEntry(entry),
            FileStructureStorageProvider.MinIO => BuildMinioConfigurationFromEntry(entry),
            FileStructureStorageProvider.S3Provider => BuildS3ConfigurationFromEntry(entry),
            _ => GetDefaultConfigurationFromSettings(entry.Key)
        };
    }

    private BlobContainerConfiguration BuildConfigurationFromStructure(FileStructures.FileStructure structure)
    {
        var providerStr = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.Provider) as string;
        if (string.IsNullOrEmpty(providerStr))
        {
            return GetDefaultConfigurationFromSettings(structure.Key);
        }

        if (!Enum.TryParse<FileStructureStorageProvider>(providerStr, ignoreCase: true, out var provider))
        {
            return GetDefaultConfigurationFromSettings(structure.Key);
        }

        return provider switch
        {
            FileStructureStorageProvider.Database => BuildDatabaseConfiguration(structure),
            FileStructureStorageProvider.FileSystem => BuildFileSystemConfiguration(structure),
            FileStructureStorageProvider.MinIO => BuildMinioConfiguration(structure),
            FileStructureStorageProvider.S3Provider => BuildS3Configuration(structure),
            _ => GetDefaultConfigurationFromSettings(structure.Key)
        };
    }

    private BlobContainerConfiguration GetDefaultConfigurationFromSettings(string? structureKey)
    {
        ResolveStructurePublicSettings(structureKey, out var baseUrl, out var isPublicAccess);
        return GetDefaultConfigurationFromSettings(structureKey, baseUrl, isPublicAccess);
    }

    private void ResolveStructurePublicSettings(string? structureKey, out string? baseUrl, out bool? isPublicAccess)
    {
        baseUrl = null;
        isPublicAccess = null;
        if (string.IsNullOrEmpty(structureKey))
        {
            return;
        }

        var cached = AsyncHelper.RunSync(() => StructureCache.GetAsync(structureKey));
        if (cached != null)
        {
            baseUrl = cached.BaseUrl;
            isPublicAccess = cached.IsPublicAccess;
            return;
        }

        var structure = AsyncHelper.RunSync(() => FileStructureRepository.FindByKeyAsync(structureKey));
        if (structure != null)
        {
            baseUrl = structure.BaseUrl;
            isPublicAccess = structure.IsPublicAccess;
        }
    }

    private BlobContainerConfiguration GetDefaultConfigurationFromSettings(
        string? structureKey,
        string? structureBaseUrl,
        bool? structureIsPublicAccess)
    {
        var defaultConfig = AsyncHelper.RunSync(() => StorageConfigProvider.GetDefaultConfigAsync());
        return BuildConfigurationFromConfigDto(defaultConfig, structureKey, structureBaseUrl, structureIsPublicAccess);
    }

    private BlobContainerConfiguration BuildConfigurationFromConfigDto(
        FileStructureStorageConfigDto dto,
        string? structureKey,
        string? structureBaseUrl = null,
        bool? structureIsPublicAccess = null)
    {
        return dto.StorageProvider switch
        {
            FileStructureStorageProvider.Database => new BlobContainerConfiguration().UseDatabase(),
            FileStructureStorageProvider.FileSystem => BuildFileSystemConfigurationFromDto(dto, structureKey),
            FileStructureStorageProvider.MinIO => BuildMinioConfigurationFromDto(dto),
            FileStructureStorageProvider.S3Provider => BuildS3ConfigurationFromDto(dto, structureBaseUrl, structureIsPublicAccess),
            _ => new BlobContainerConfiguration().UseDatabase()
        };
    }

    private BlobContainerConfiguration BuildFileSystemConfigurationFromDto(FileStructureStorageConfigDto dto, string? structureKey)
    {
        var customPath = dto.FileSystemBasePath;
        var key = NormalizeStructureKeyForPath(structureKey ?? "general");
        var fullBasePath = BuildFileSystemBasePath(key, customPath);

        var config = new BlobContainerConfiguration();
        config.UseFileSystem(fs =>
        {
            fs.BasePath = fullBasePath;
            fs.AppendContainerNameToBasePath = false;
        });
        return config;
    }

    private BlobContainerConfiguration BuildMinioConfigurationFromDto(FileStructureStorageConfigDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MinioEndPoint) || string.IsNullOrWhiteSpace(dto.MinioBucketName)
            || string.IsNullOrWhiteSpace(dto.MinioAccessKey) || string.IsNullOrWhiteSpace(dto.MinioSecretKey))
        {
            return new BlobContainerConfiguration().UseDatabase();
        }

        var config = new BlobContainerConfiguration();
        config.UseMinio(minio =>
        {
            minio.EndPoint = dto.MinioEndPoint;
            minio.BucketName = dto.MinioBucketName;
            minio.AccessKey = dto.MinioAccessKey;
            minio.SecretKey = dto.MinioSecretKey;
        });
        return config;
    }

    private BlobContainerConfiguration BuildS3ConfigurationFromDto(
        FileStructureStorageConfigDto dto,
        string? structureBaseUrl = null,
        bool? structureIsPublicAccess = null)
    {
        if (string.IsNullOrWhiteSpace(dto.S3ContainerName) || string.IsNullOrWhiteSpace(dto.S3AccessKeyId) || string.IsNullOrWhiteSpace(dto.S3SecretAccessKey))
        {
            return new BlobContainerConfiguration().UseDatabase();
        }

        var config = new BlobContainerConfiguration();
        config.UseS3(s3 =>
        {
            if (!string.IsNullOrWhiteSpace(dto.S3EndPoint))
            {
                s3.Endpoint = dto.S3EndPoint.TrimEnd('/');
                s3.ForcePathStyle = true;
            }
            s3.Region = string.IsNullOrWhiteSpace(dto.S3Region) ? "us-east-1" : dto.S3Region;
            s3.ContainerName = dto.S3ContainerName;
            s3.AccessKeyId = dto.S3AccessKeyId;
            s3.SecretAccessKey = dto.S3SecretAccessKey;
            ApplyS3PublicAccess(s3, structureIsPublicAccess ?? false, structureBaseUrl);
        });
        return config;
    }

    /// <summary>
    /// Marks the S3 container public and sets PublicBaseUrl from structure BaseUrl or derived S3 URL.
    /// When IsPublicAccess is true, uploads use public-read ACL so objects are reachable without the app API.
    /// </summary>
    private static void ApplyS3PublicAccess(S3BlobProviderConfiguration s3, bool isPublicAccess, string? structureBaseUrl)
    {
        s3.IsPublicAccess = isPublicAccess;
        if (!isPublicAccess)
        {
            return;
        }

        s3.PublicBaseUrl = S3PublicUrlBuilder.ResolvePublicBaseUrl(
            structureBaseUrl,
            s3.Endpoint,
            s3.Region,
            s3.ContainerName,
            isPublicAccess: true);
    }

    private BlobContainerConfiguration BuildDatabaseConfiguration(FileStructures.FileStructure structure)
    {
        var config = new BlobContainerConfiguration();
        config.UseDatabase();
        // Note: Custom connection string per structure would require a custom DatabaseBlobProvider.
        // For now, Database provider uses the default connection. ConnectionString in ExtraProperties
        // is stored for future use.
        return config;
    }

    private static BlobContainerConfiguration BuildDatabaseConfigurationFromEntry(Caching.StructureCacheEntry entry)
    {
        var config = new BlobContainerConfiguration();
        config.UseDatabase();
        return config;
    }

    private BlobContainerConfiguration BuildFileSystemConfiguration(FileStructures.FileStructure structure)
    {
        var customPath = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.FileSystemBasePath) as string;
        var structureKey = NormalizeStructureKeyForPath(structure.Key);
        var fullBasePath = BuildFileSystemBasePath(structureKey, customPath);

        var config = new BlobContainerConfiguration();
        config.UseFileSystem(fs =>
        {
            fs.BasePath = fullBasePath;
            fs.AppendContainerNameToBasePath = false;
        });
        return config;
    }

    private BlobContainerConfiguration BuildFileSystemConfigurationFromEntry(Caching.StructureCacheEntry entry)
    {
        var customPath = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.FileSystemBasePath) as string;
        var structureKey = NormalizeStructureKeyForPath(entry.Key);
        var fullBasePath = BuildFileSystemBasePath(structureKey, customPath);

        var config = new BlobContainerConfiguration();
        config.UseFileSystem(fs =>
        {
            fs.BasePath = fullBasePath;
            fs.AppendContainerNameToBasePath = false;
        });
        return config;
    }

    /// <summary>
    /// Builds the standardized FileSystem base path.
    /// Standard: assets/{structure-name} (ABP adds host/tenant, our blob adds year/month).
    /// Custom: assets/{custom-path} when user provides a custom path.
    /// </summary>
    private static string BuildFileSystemBasePath(string structureKey, string? customPath)
    {
        var prefix = FileStructureStorageConstants.AssetsPathPrefix;

        if (!string.IsNullOrWhiteSpace(customPath))
        {
            var normalized = NormalizeCustomPath(customPath);
            return string.IsNullOrEmpty(normalized)
                ? prefix
                : Path.Combine(prefix, normalized);
        }

        return Path.Combine(prefix, structureKey);
    }

    private static string NormalizeStructureKeyForPath(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return "general";
        return key.Replace(".", "-", StringComparison.Ordinal).ToLowerInvariant();
    }

    private static string NormalizeCustomPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var normalized = path.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar);
        return normalized;
    }

    private BlobContainerConfiguration BuildMinioConfiguration(FileStructures.FileStructure structure)
    {
        var endPoint = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioEndPoint) as string;
        var bucketName = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioBucketName) as string;
        var accessKeyEnc = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioAccessKey) as string;
        var secretKeyEnc = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioSecretKey) as string;

        if (string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(bucketName)
            || string.IsNullOrWhiteSpace(accessKeyEnc) || string.IsNullOrWhiteSpace(secretKeyEnc))
        {
            return GetDefaultConfigurationFromSettings(structure.Key);
        }

        var accessKey = Encryption.DecryptSensitiveValue(accessKeyEnc);
        var secretKey = Encryption.DecryptSensitiveValue(secretKeyEnc);
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            return GetDefaultConfigurationFromSettings(structure.Key);
        }

        var config = new BlobContainerConfiguration();
        config.UseMinio(minio =>
        {
            minio.EndPoint = endPoint;
            minio.BucketName = bucketName;
            minio.AccessKey = accessKey;
            minio.SecretKey = secretKey;
        });
        return config;
    }

    private BlobContainerConfiguration BuildMinioConfigurationFromEntry(Caching.StructureCacheEntry entry)
    {
        var endPoint = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioEndPoint) as string;
        var bucketName = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioBucketName) as string;
        var accessKeyEnc = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioAccessKey) as string;
        var secretKeyEnc = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.MinioSecretKey) as string;

        if (string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(bucketName)
            || string.IsNullOrWhiteSpace(accessKeyEnc) || string.IsNullOrWhiteSpace(secretKeyEnc))
        {
            return GetDefaultConfigurationFromSettings(entry.Key);
        }

        var accessKey = Encryption.DecryptSensitiveValue(accessKeyEnc);
        var secretKey = Encryption.DecryptSensitiveValue(secretKeyEnc);
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            return GetDefaultConfigurationFromSettings(entry.Key);
        }

        var config = new BlobContainerConfiguration();
        config.UseMinio(minio =>
        {
            minio.EndPoint = endPoint;
            minio.BucketName = bucketName;
            minio.AccessKey = accessKey;
            minio.SecretKey = secretKey;
        });
        return config;
    }

    private BlobContainerConfiguration BuildS3Configuration(FileStructures.FileStructure structure)
    {
        var endPoint = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3Endpoint) as string;
        var region = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3Region) as string;
        var containerName = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3ContainerName) as string;
        var accessKeyEnc = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3AccessKeyId) as string;
        var secretKeyEnc = structure.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3SecretAccessKey) as string;

        if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(accessKeyEnc) || string.IsNullOrWhiteSpace(secretKeyEnc))
        {
            return GetDefaultConfigurationFromSettings(structure.Key, structure.BaseUrl, structure.IsPublicAccess);
        }

        var accessKey = Encryption.DecryptSensitiveValue(accessKeyEnc);
        var secretKey = Encryption.DecryptSensitiveValue(secretKeyEnc);
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            return GetDefaultConfigurationFromSettings(structure.Key, structure.BaseUrl, structure.IsPublicAccess);
        }

        var config = new BlobContainerConfiguration();
        config.UseS3(s3 =>
        {
            if (!string.IsNullOrWhiteSpace(endPoint))
            {
                s3.Endpoint = endPoint.TrimEnd('/');
                s3.ForcePathStyle = true;
            }
            s3.Region = string.IsNullOrWhiteSpace(region) ? "us-east-1" : region;
            s3.ContainerName = containerName;
            s3.AccessKeyId = accessKey;
            s3.SecretAccessKey = secretKey;
            ApplyS3PublicAccess(s3, structure.IsPublicAccess, structure.BaseUrl);
        });
        return config;
    }

    private BlobContainerConfiguration BuildS3ConfigurationFromEntry(Caching.StructureCacheEntry entry)
    {
        var endPoint = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3Endpoint) as string;
        var region = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3Region) as string;
        var containerName = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3ContainerName) as string;
        var accessKeyEnc = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3AccessKeyId) as string;
        var secretKeyEnc = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.S3SecretAccessKey) as string;

        if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(accessKeyEnc) || string.IsNullOrWhiteSpace(secretKeyEnc))
        {
            return GetDefaultConfigurationFromSettings(entry.Key, entry.BaseUrl, entry.IsPublicAccess);
        }

        var accessKey = Encryption.DecryptSensitiveValue(accessKeyEnc);
        var secretKey = Encryption.DecryptSensitiveValue(secretKeyEnc);
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            return GetDefaultConfigurationFromSettings(entry.Key, entry.BaseUrl, entry.IsPublicAccess);
        }

        var config = new BlobContainerConfiguration();
        config.UseS3(s3 =>
        {
            if (!string.IsNullOrWhiteSpace(endPoint))
            {
                s3.Endpoint = endPoint.TrimEnd('/');
                s3.ForcePathStyle = true;
            }
            s3.Region = string.IsNullOrWhiteSpace(region) ? "us-east-1" : region;
            s3.ContainerName = containerName;
            s3.AccessKeyId = accessKey;
            s3.SecretAccessKey = secretKey;
            ApplyS3PublicAccess(s3, entry.IsPublicAccess, entry.BaseUrl);
        });
        return config;
    }
}
