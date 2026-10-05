using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Download, stream, thumbnail URL building and blob content resolution for file items.
/// Extracted from <see cref="FileItemAppService"/> to reduce god-class surface.
/// </summary>
public class FileItemBlobAccessService
{
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IStructureCache _structureCache;
    private readonly IStructureBlobContainerProvider _structureBlobContainerProvider;
    private readonly IFileAccessTokenService _fileAccessTokenService;
    private readonly IS3PublicBlobUrlProvider _s3PublicBlobUrlProvider;
    private readonly IS3PresignedUrlProvider _s3PresignedUrlProvider;
    private readonly FileManagerOptions _options;
    private readonly ILogger<FileItemBlobAccessService> _logger;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDataFilter _dataFilter;

    public FileItemBlobAccessService(
        IFileItemRepository fileItemRepository,
        IStructureCache structureCache,
        IStructureBlobContainerProvider structureBlobContainerProvider,
        IFileAccessTokenService fileAccessTokenService,
        IS3PublicBlobUrlProvider s3PublicBlobUrlProvider,
        IS3PresignedUrlProvider s3PresignedUrlProvider,
        IOptions<FileManagerOptions> options,
        ILogger<FileItemBlobAccessService> logger,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IDataFilter dataFilter)
    {
        _fileItemRepository = fileItemRepository;
        _structureCache = structureCache;
        _structureBlobContainerProvider = structureBlobContainerProvider;
        _fileAccessTokenService = fileAccessTokenService;
        _s3PublicBlobUrlProvider = s3PublicBlobUrlProvider;
        _s3PresignedUrlProvider = s3PresignedUrlProvider;
        _options = options.Value;
        _logger = logger;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _dataFilter = dataFilter;
    }

    public virtual async Task<string> GetDownloadUrlAsync(FileItem fileItem)
    {
        if (!fileItem.StorageProvider.HasValue
            && _s3PublicBlobUrlProvider.TryGetPublicUrl(GetContainerName(fileItem.StructureKey), fileItem.BlobName, fileItem.TenantId, out var directUrl))
        {
            return directUrl;
        }

        var entry = await _structureCache.GetAsync(fileItem.StructureKey);
        var baseUrl = _options.BaseUrl ?? "/";
        var path = $"{baseUrl.TrimEnd('/')}/api/file-manager/file-items/{fileItem.Id}/download";
        var url = entry?.IsPublicAccess == true
            ? path
            : AppendAccessTokenIfConfigured(path, fileItem.Id);
        if (_currentTenant.IsAvailable)
        {
            url += (url.Contains('?') ? "&" : "?") + $"__tenant={_currentTenant.Id:N}";
        }

        return url;
    }

    public virtual async Task<string> GetThumbnailUrlAsync(FileItem fileItem)
    {
        if (string.IsNullOrEmpty(fileItem.ThumbnailBlobName))
        {
            throw new UserFriendlyException("Thumbnail not available for this file item");
        }

        if (!fileItem.StorageProvider.HasValue
            && _s3PublicBlobUrlProvider.TryGetPublicUrl(GetContainerName(fileItem.StructureKey), fileItem.ThumbnailBlobName, fileItem.TenantId, out var directUrl))
        {
            return directUrl;
        }

        var entry = await _structureCache.GetAsync(fileItem.StructureKey);
        var baseUrl = _options.BaseUrl ?? "/";
        var path = $"{baseUrl.TrimEnd('/')}/api/file-manager/file-items/{fileItem.Id}/thumbnail";
        var url = entry?.IsPublicAccess == true
            ? path
            : AppendAccessTokenIfConfigured(path, fileItem.Id);
        if (_currentTenant.IsAvailable)
        {
            url += (url.Contains('?') ? "&" : "?") + $"__tenant={_currentTenant.Id:N}";
        }

        return url;
    }

    public virtual async Task<string> GetStreamUrlAsync(FileItem fileItem)
    {
        if (!fileItem.StorageProvider.HasValue
            && _s3PublicBlobUrlProvider.TryGetPublicUrl(GetContainerName(fileItem.StructureKey), fileItem.BlobName, fileItem.TenantId, out var directUrl))
        {
            return directUrl;
        }

        var entry = await _structureCache.GetAsync(fileItem.StructureKey);
        var baseUrl = _options.BaseUrl ?? "/";
        var path = $"{baseUrl.TrimEnd('/')}/api/file-manager/file-items/{fileItem.Id}/stream";
        var url = entry?.IsPublicAccess == true
            ? path
            : AppendAccessTokenIfConfigured(path, fileItem.Id);
        if (_currentTenant.IsAvailable)
        {
            url += (url.Contains('?') ? "&" : "?") + $"__tenant={_currentTenant.Id:N}";
        }

        return url;
    }

    public virtual async Task<string> GetTemporaryAccessUrlAsync(FileItem fileItem, int durationMinutes)
    {
        if (fileItem.StorageProvider.HasValue)
        {
            return await GetDownloadUrlAsync(fileItem);
        }

        var entry = await _structureCache.GetAsync(fileItem.StructureKey);
        var providerStr = entry?.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.Provider) as string;
        var isS3 = string.Equals(providerStr, "S3Provider", StringComparison.OrdinalIgnoreCase);

        if (isS3 && entry is { IsPublicAccess: false } && !string.IsNullOrWhiteSpace(entry.BaseUrl))
        {
            var validity = TimeSpan.FromMinutes(Math.Clamp(durationMinutes, 1, 10080));
            var containerName = GetContainerName(fileItem.StructureKey);
            var presignedUrl = await _s3PresignedUrlProvider.GetPresignedDownloadUrlAsync(
                containerName, fileItem.BlobName, fileItem.TenantId, validity);
            if (!string.IsNullOrEmpty(presignedUrl))
            {
                if (_currentTenant.IsAvailable)
                {
                    presignedUrl += (presignedUrl.Contains('?') ? "&" : "?") + $"__tenant={_currentTenant.Id:N}";
                }

                return presignedUrl;
            }
        }

        return await GetDownloadUrlAsync(fileItem);
    }

    public virtual async Task<FileContentResultDto> GetDownloadContentAsync(Guid id, string? token)
    {
        var (metadata, isForbidden) = await ResolveAccessMetadataAsync(id, token);
        if (isForbidden)
        {
            return new FileContentResultDto { IsForbidden = true };
        }

        if (metadata == null)
        {
            return new FileContentResultDto();
        }

        return await ReadBlobContentAsync(id, metadata);
    }

    /// <summary>
    /// Reads bytes for trusted in-process module callers.
    /// Does not apply token, public-structure, or HTTP-user checks.
    /// The current tenant data filter still applies.
    /// Callers must authorize the file in their own domain.
    /// </summary>
    public virtual async Task<FileContentResultDto> GetContentForIntegrationAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        return await ReadBlobContentAsync(id, MapToStreamMetadata(fileItem));
    }

    public virtual async Task<StreamContentResultDto> GetStreamContentAsync(Guid id, string? token)
    {
        var (metadata, isForbidden) = await ResolveAccessMetadataAsync(id, token);
        if (isForbidden)
        {
            return new StreamContentResultDto { IsForbidden = true };
        }

        if (metadata == null)
        {
            return new StreamContentResultDto();
        }

        Stream? stream;
        using (_currentTenant.Change(metadata.TenantId))
        {
            var container = await _structureBlobContainerProvider.GetContainerAsync(
                metadata.StructureKey,
                metadata.StorageProvider);
            stream = await container.GetOrNullAsync(metadata.BlobName);
        }

        if (stream == null)
        {
            return new StreamContentResultDto();
        }

        return new StreamContentResultDto
        {
            Content = new StreamContentDto
            {
                Stream = stream,
                MimeType = NormalizeMimeType(metadata.MimeType)
            }
        };
    }

    public virtual async Task<FileContentResultDto> GetThumbnailContentAsync(Guid id, string? token)
    {
        var (metadata, isForbidden) = await ResolveAccessMetadataAsync(id, token);
        if (isForbidden)
        {
            return new FileContentResultDto { IsForbidden = true };
        }

        if (metadata == null)
        {
            return new FileContentResultDto();
        }

        if (string.IsNullOrEmpty(metadata.ThumbnailBlobName))
        {
            return new FileContentResultDto();
        }

        using (_currentTenant.Change(metadata.TenantId))
        {
            var container = await _structureBlobContainerProvider.GetContainerAsync(
                metadata.StructureKey,
                metadata.StorageProvider);
            await using (var stream = await container.GetOrNullAsync(metadata.ThumbnailBlobName))
            {
                if (stream == null)
                {
                    _logger.LogWarning("Thumbnail blob not found: FileId={FileId}, ThumbnailBlobName={ThumbnailBlobName}, StructureKey={StructureKey}, TenantId={TenantId}",
                        id, metadata.ThumbnailBlobName, metadata.StructureKey, metadata.TenantId);
                    return new FileContentResultDto();
                }

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var blob = ms.ToArray();
                return new FileContentResultDto
                {
                    Content = new FileContentDto
                    {
                        Content = blob,
                        MimeType = "image/webp",
                        FileName = "thumb.webp"
                    }
                };
            }
        }
    }

    protected virtual async Task<FileContentResultDto> ReadBlobContentAsync(Guid id, FileStreamMetadataDto metadata)
    {
        using (_currentTenant.Change(metadata.TenantId))
        {
            var container = await _structureBlobContainerProvider.GetContainerAsync(
                metadata.StructureKey,
                metadata.StorageProvider);
            await using (var stream = await container.GetOrNullAsync(metadata.BlobName))
            {
                if (stream == null)
                {
                    _logger.LogWarning("Blob not found for download: FileId={FileId}, BlobName={BlobName}, StructureKey={StructureKey}, TenantId={TenantId}",
                        id, metadata.BlobName, metadata.StructureKey, metadata.TenantId);
                    return new FileContentResultDto();
                }

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var blob = ms.ToArray();
                return new FileContentResultDto
                {
                    Content = new FileContentDto
                    {
                        Content = blob,
                        MimeType = NormalizeMimeType(metadata.MimeType),
                        FileName = metadata.OriginalName
                    }
                };
            }
        }
    }

    protected virtual string AppendAccessTokenIfConfigured(string path, Guid fileItemId)
    {
        if (!_fileAccessTokenService.TryGenerateToken(fileItemId, out var token))
        {
            return path;
        }

        return path + (path.Contains('?') ? "&" : "?") + "token=" + Uri.EscapeDataString(token);
    }

    public static string GetContainerName(string? structureKey) =>
        string.IsNullOrEmpty(structureKey)
            ? FileStructureStorageConstants.DefaultContainerName
            : FileStructureStorageConstants.ContainerNamePrefix + structureKey;

    /// <summary>
    /// Resolves file metadata for download, stream, and thumbnail.
    /// A matching access token wins, then a public structure, then an authenticated caller in the file's tenant.
    /// Public structure access is checked for anonymous and authenticated callers.
    /// Returns (null, false) when the item is missing or outside the caller's tenant.
    /// Returns (null, true) when the item exists in the current tenant and the caller may not read it.
    /// Uses <see cref="IFileItemRepository.FindAsync"/> so a missing item is an empty result, not an exception.
    /// </summary>
    protected virtual async Task<(FileStreamMetadataDto? metadata, bool isForbidden)> ResolveAccessMetadataAsync(Guid id, string? token)
    {
        var fileItem = await FindFileIgnoringTenantAsync(id);
        if (fileItem == null)
        {
            return (null, false);
        }

        if (TokenMatchesFile(id, token) || await IsPublicStructureAsync(fileItem))
        {
            return (MapToStreamMetadata(fileItem), false);
        }

        if (_currentUser.IsAuthenticated && IsInCurrentTenant(fileItem))
        {
            return (MapToStreamMetadata(fileItem), false);
        }

        if (IsInCurrentTenant(fileItem))
        {
            return (null, true);
        }

        return (null, false);
    }

    /// <summary>
    /// A token authorizes only the file id it was issued for.
    /// A token for a different id does not select that other file.
    /// </summary>
    protected virtual bool TokenMatchesFile(Guid id, string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return _fileAccessTokenService.TryValidateToken(token, out var fileId) && fileId == id;
    }

    protected virtual async Task<bool> IsPublicStructureAsync(FileItem fileItem)
    {
        if (string.IsNullOrEmpty(fileItem.StructureKey))
        {
            return false;
        }

        return await _structureCache.IsPublicAccessAsync(fileItem.StructureKey);
    }

    protected virtual async Task<FileItem?> FindFileIgnoringTenantAsync(Guid id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await _fileItemRepository.FindAsync(id);
        }
    }

    protected virtual bool IsInCurrentTenant(FileItem fileItem)
    {
        if (_currentTenant.IsAvailable)
        {
            return fileItem.TenantId == _currentTenant.Id;
        }

        return fileItem.TenantId == null;
    }

    private const string DefaultContentType = "application/octet-stream";

    private static string NormalizeMimeType(string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return DefaultContentType;
        }

        return mimeType.Trim();
    }

    private static FileStreamMetadataDto MapToStreamMetadata(FileItem fileItem) =>
        new()
        {
            BlobName = fileItem.BlobName,
            MimeType = fileItem.MimeType,
            ThumbnailBlobName = fileItem.ThumbnailBlobName,
            OriginalName = fileItem.OriginalName,
            StructureKey = fileItem.StructureKey,
            TenantId = fileItem.TenantId,
            StorageProvider = fileItem.StorageProvider
        };
}
