using Microsoft.Extensions.Configuration;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Services;

/// <summary>
/// Resolves file manager API URLs using RemoteServices:FileManager:BaseUrl or RemoteServices:Default:BaseUrl.
/// Appends signed tokens to thumbnail/stream URLs so img/video elements can load media without auth headers.
/// </summary>
public class FileItemUrlProvider : IFileItemUrlProvider
{
    private const string FileItemsPath = "api/file-manager/file-items";

    private readonly string _apiBaseUrl;
    private readonly IFileAccessTokenService _fileAccessTokenService;

    public FileItemUrlProvider(IConfiguration configuration, IFileAccessTokenService fileAccessTokenService)
    {
        var baseUrl = configuration["RemoteServices:FileManager:BaseUrl"]
                      ?? configuration["RemoteServices:Default:BaseUrl"]
                      ?? "";
        _apiBaseUrl = (baseUrl ?? "").TrimEnd('/');
        if (!string.IsNullOrEmpty(_apiBaseUrl) && !_apiBaseUrl.EndsWith("/"))
        {
            _apiBaseUrl += "/";
        }
        _fileAccessTokenService = fileAccessTokenService;
    }

    /// <summary>
    /// Base URL of the file manager API. Empty string means same-origin (relative paths).
    /// </summary>
    public string ApiBaseUrl => _apiBaseUrl;

    public string GetThumbnailUrl(Guid fileItemId, long? cacheBust = null, string? structureBaseUrl = null, bool structureIsPublicAccess = false, string? thumbnailBlobName = null, Guid? tenantId = null, string? structureStorageProvider = null)
    {
        if (TryBuildS3DirectUrl(structureBaseUrl, structureIsPublicAccess, thumbnailBlobName, tenantId, structureStorageProvider, out var directUrl))
        {
            return AppendCacheBust(directUrl, cacheBust);
        }
        // Structure BaseUrl is for direct S3 URLs only; API paths must use API base
        var path = string.IsNullOrEmpty(_apiBaseUrl)
            ? $"/{FileItemsPath}/{fileItemId}/thumbnail"
            : $"{_apiBaseUrl}{FileItemsPath}/{fileItemId}/thumbnail";
        var url = AppendAccessTokenIfNeeded(path, fileItemId, structureIsPublicAccess);
        return AppendCacheBust(url, cacheBust);
    }

    public string GetDownloadUrl(Guid fileItemId, string? structureBaseUrl = null, bool structureIsPublicAccess = false, string? blobName = null, Guid? tenantId = null, string? structureStorageProvider = null)
    {
        if (TryBuildS3DirectUrl(structureBaseUrl, structureIsPublicAccess, blobName, tenantId, structureStorageProvider, out var directUrl))
            return directUrl;
        // Structure BaseUrl is for direct S3 URLs only; API paths must use API base
        var path = string.IsNullOrEmpty(_apiBaseUrl)
            ? $"/{FileItemsPath}/{fileItemId}/download"
            : $"{_apiBaseUrl}{FileItemsPath}/{fileItemId}/download";
        return AppendAccessTokenIfNeeded(path, fileItemId, structureIsPublicAccess);
    }

    public string GetStreamUrl(Guid fileItemId, long? cacheBust = null, string? structureBaseUrl = null, bool structureIsPublicAccess = false, string? blobName = null, Guid? tenantId = null, string? structureStorageProvider = null)
    {
        if (TryBuildS3DirectUrl(structureBaseUrl, structureIsPublicAccess, blobName, tenantId, structureStorageProvider, out var directUrl))
        {
            return AppendCacheBust(directUrl, cacheBust);
        }
        // Structure BaseUrl is for direct S3 URLs only; API paths must use API base
        var path = string.IsNullOrEmpty(_apiBaseUrl)
            ? $"/{FileItemsPath}/{fileItemId}/stream"
            : $"{_apiBaseUrl}{FileItemsPath}/{fileItemId}/stream";
        var url = AppendAccessTokenIfNeeded(path, fileItemId, structureIsPublicAccess);
        return AppendCacheBust(url, cacheBust);
    }

    private string AppendAccessTokenIfNeeded(string path, Guid fileItemId, bool structureIsPublicAccess)
    {
        if (structureIsPublicAccess || !_fileAccessTokenService.TryGenerateToken(fileItemId, out var token))
        {
            return path;
        }

        return path + (path.Contains('?') ? "&" : "?") + "token=" + Uri.EscapeDataString(token);
    }

    private static bool TryBuildS3DirectUrl(string? structureBaseUrl, bool structureIsPublicAccess, string? blobName, Guid? tenantId, string? structureStorageProvider, out string url)
    {
        url = null!;
        if (!structureIsPublicAccess || string.IsNullOrWhiteSpace(structureBaseUrl) || string.IsNullOrWhiteSpace(blobName))
            return false;
        if (!string.Equals(structureStorageProvider, "S3Provider", StringComparison.OrdinalIgnoreCase))
            return false;
        var s3Key = tenantId == null ? $"host/{blobName}" : $"tenants/{tenantId.Value:D}/{blobName}";
        url = $"{structureBaseUrl!.TrimEnd('/')}/{s3Key}";
        return true;
    }

    private static string AppendCacheBust(string url, long? cacheBust)
    {
        if (!cacheBust.HasValue) return url;
        return url + (url.Contains('?') ? "&" : "?") + "_=" + Uri.EscapeDataString(cacheBust.Value.ToString());
    }
}
