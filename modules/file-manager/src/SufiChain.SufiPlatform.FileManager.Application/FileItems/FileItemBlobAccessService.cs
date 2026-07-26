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
        if (_s3PublicBlobUrlProvider.TryGetPublicUrl(GetContainerName(fileItem.StructureKey), fileItem.BlobName, fileItem.TenantId, out var directUrl))
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

        if (_s3PublicBlobUrlProvider.TryGetPublicUrl(GetContainerName(fileItem.StructureKey), fileItem.ThumbnailBlobName, fileItem.TenantId, out var directUrl))
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
        if (_s3PublicBlobUrlProvider.TryGetPublicUrl(GetContainerName(fileItem.StructureKey), fileItem.BlobName, fileItem.TenantId, out var directUrl))
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

        using (_currentTenant.Change(metadata.TenantId))
        {
            var container = await _structureBlobContainerProvider.GetContainerAsync(metadata.StructureKey);
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
                        MimeType = metadata.MimeType,
                        FileName = metadata.OriginalName
                    }
                };
            }
        }
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
            var container = await _structureBlobContainerProvider.GetContainerAsync(metadata.StructureKey);
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
                MimeType = metadata.MimeType
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
            var container = await _structureBlobContainerProvider.GetContainerAsync(metadata.StructureKey);
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
    /// Resolves file metadata for download/stream/thumbnail: token → public access → authenticated.
    /// Returns (null, true) when access is forbidden; (null, false) when not found; (metadata, false) when ok.
    /// </summary>
    protected virtual async Task<(FileStreamMetadataDto? metadata, bool isForbidden)> ResolveAccessMetadataAsync(Guid id, string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            var meta = await GetStreamMetadataByTokenAsync(token);
            return (meta, false);
        }

        if (!_currentUser.IsAuthenticated)
        {
            var publicMeta = await TryGetMetadataForPublicAccessAsync(id);
            if (publicMeta == null)
            {
                return (null, true);
            }

            return (publicMeta, false);
        }

        var fileItem = await _fileItemRepository.GetAsync(id);
        return (MapToStreamMetadata(fileItem), false);
    }

    protected virtual async Task<FileStreamMetadataDto?> GetStreamMetadataByTokenAsync(string token)
    {
        if (!_fileAccessTokenService.TryValidateToken(token, out var fileId))
        {
            return null;
        }

        FileItem? fileItem;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            fileItem = await _fileItemRepository.FindAsync(fileId);
        }

        if (fileItem == null)
        {
            return null;
        }

        return MapToStreamMetadata(fileItem);
    }

    protected virtual async Task<FileStreamMetadataDto?> TryGetMetadataForPublicAccessAsync(Guid id)
    {
        FileItem? fileItem;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            fileItem = await _fileItemRepository.FindAsync(id);
        }

        if (fileItem == null || string.IsNullOrEmpty(fileItem.StructureKey))
        {
            return null;
        }

        if (!await _structureCache.IsPublicAccessAsync(fileItem.StructureKey))
        {
            return null;
        }

        return MapToStreamMetadata(fileItem);
    }

    private static FileStreamMetadataDto MapToStreamMetadata(FileItem fileItem) =>
        new()
        {
            BlobName = fileItem.BlobName,
            MimeType = fileItem.MimeType,
            ThumbnailBlobName = fileItem.ThumbnailBlobName,
            OriginalName = fileItem.OriginalName,
            StructureKey = fileItem.StructureKey,
            TenantId = fileItem.TenantId
        };
}
