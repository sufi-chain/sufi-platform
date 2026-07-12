using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;

/// <summary>
/// Default implementation of IFilePublicUrlResolver using the FileItem app service.
/// </summary>
public class FilePublicUrlResolver : IFilePublicUrlResolver
{
    private readonly IFileItemAppService _fileItemAppService;

    public FilePublicUrlResolver(IFileItemAppService fileItemAppService)
    {
        _fileItemAppService = fileItemAppService;
    }

    public async Task<string?> GetDownloadUrlAsync(Guid fileId)
    {
        try
        {
            return await _fileItemAppService.GetDownloadUrlAsync(fileId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetThumbnailUrlAsync(Guid fileId, FileImageSize? size = null)
    {
        try
        {
            return await _fileItemAppService.GetThumbnailUrlAsync(fileId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetStreamUrlAsync(Guid fileId)
    {
        try
        {
            return await _fileItemAppService.GetDownloadUrlAsync(fileId); // Use download URL for streaming
        }
        catch
        {
            return null;
        }
    }

    public async Task<Dictionary<FileImageSize, string>> GetImageVariantsAsync(Guid fileId)
    {
        var variants = new Dictionary<FileImageSize, string>();

        try
        {
            // Get thumbnail URL for each size variant
            var thumbnailUrl = await _fileItemAppService.GetThumbnailUrlAsync(fileId);
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                // For now, use the same thumbnail URL for all sizes
                // In future, could support actual size variants from image processing
                variants[FileImageSize.Thumbnail] = thumbnailUrl;
                variants[FileImageSize.Small] = thumbnailUrl;
                variants[FileImageSize.Medium] = thumbnailUrl;
            }

            var downloadUrl = await _fileItemAppService.GetDownloadUrlAsync(fileId);
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                variants[FileImageSize.Large] = downloadUrl;
                variants[FileImageSize.Original] = downloadUrl;
            }
        }
        catch
        {
            // Return empty variants on error
        }

        return variants;
    }

    public async Task<FilePublicInfo?> GetFilePublicInfoAsync(Guid fileId)
    {
        try
        {
            var fileItem = await _fileItemAppService.GetAsync(fileId);
            if (fileItem == null)
            {
                return null;
            }

            // Cache-bust so edited files show updated content (browser would otherwise show stale cached image)
            var cacheBust = (fileItem.LastModificationTime ?? fileItem.CreationTime).Ticks;

            var info = new FilePublicInfo
            {
                Id = fileItem.Id,
                FileName = fileItem.Name,
                Alt = fileItem.Alt,
                MimeType = fileItem.MimeType,
                Size = fileItem.Size,
                IsImage = IsImageMimeType(fileItem.MimeType),
                IsVideo = IsVideoMimeType(fileItem.MimeType),
                IsAudio = IsAudioMimeType(fileItem.MimeType)
            };

            // Get URLs based on file type, with cache-busting
            if (info.IsImage)
            {
                info.ThumbnailUrl = AppendCacheBust(await GetThumbnailUrlAsync(fileId), cacheBust);
                info.DownloadUrl = AppendCacheBust(await GetDownloadUrlAsync(fileId), cacheBust);
                info.SizeVariants = await GetImageVariantsWithCacheBustAsync(fileId, cacheBust);
            }
            else if (info.IsVideo || info.IsAudio)
            {
                info.StreamUrl = AppendCacheBust(await GetStreamUrlAsync(fileId), cacheBust);
                info.DownloadUrl = AppendCacheBust(await GetDownloadUrlAsync(fileId), cacheBust);

                if (info.IsVideo)
                {
                    info.ThumbnailUrl = AppendCacheBust(await GetThumbnailUrlAsync(fileId), cacheBust);
                }
            }
            else
            {
                info.DownloadUrl = AppendCacheBust(await GetDownloadUrlAsync(fileId), cacheBust);
            }

            return info;
        }
        catch
        {
            return null;
        }
    }

    private static string? AppendCacheBust(string? url, long ticks)
    {
        if (string.IsNullOrEmpty(url)) return url;
        var sep = url.Contains('?') ? "&" : "?";
        return $"{url}{sep}_={ticks}";
    }

    private async Task<Dictionary<FileImageSize, string>> GetImageVariantsWithCacheBustAsync(Guid fileId, long cacheBust)
    {
        var variants = await GetImageVariantsAsync(fileId);
        var result = new Dictionary<FileImageSize, string>();
        foreach (var kv in variants)
        {
            var url = AppendCacheBust(kv.Value, cacheBust);
            if (!string.IsNullOrEmpty(url))
            {
                result[kv.Key] = url;
            }
        }
        return result;
    }

    private static bool IsImageMimeType(string mimeType)
    {
        return mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVideoMimeType(string mimeType)
    {
        return mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAudioMimeType(string mimeType)
    {
        return mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
    }
}
