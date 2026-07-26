using System;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Cached ZIP download payload for short-lived token-based downloads.
/// </summary>
[Serializable]
public class ZipDownloadCacheItem
{
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string FileName { get; set; } = default!;

    public long FileSize { get; set; }

    public int FileCount { get; set; }

    public Guid? UserId { get; set; }
}

/// <summary>
/// ZIP download content returned by <see cref="IFileManagerAppService.GetZipDownloadAsync"/>.
/// </summary>
public class ZipDownloadContentDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string FileName { get; set; } = default!;

    public string ContentType { get; set; } = "application/zip";
}
