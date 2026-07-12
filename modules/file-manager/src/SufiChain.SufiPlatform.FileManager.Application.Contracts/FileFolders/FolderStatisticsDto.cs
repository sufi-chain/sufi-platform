using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Statistics for a folder
/// </summary>
public class FolderStatisticsDto
{
    /// <summary>
    /// Folder ID
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Folder path
    /// </summary>
    public string Path { get; set; } = default!;

    /// <summary>
    /// Total number of files
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Total number of subfolders
    /// </summary>
    public int TotalFolders { get; set; }

    /// <summary>
    /// Total size in bytes
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Formatted total size
    /// </summary>
    public string FormattedSize { get; set; } = default!;

    /// <summary>
    /// Breakdown by file type
    /// </summary>
    public List<FileTypeStatDto> FileTypeStats { get; set; } = new();

    /// <summary>
    /// Date of oldest file
    /// </summary>
    public DateTime? OldestFile { get; set; }

    /// <summary>
    /// Date of newest file
    /// </summary>
    public DateTime? NewestFile { get; set; }
}

/// <summary>
/// Statistics by file type
/// </summary>
public class FileTypeStatDto
{
    /// <summary>
    /// File type name
    /// </summary>
    public string FileType { get; set; } = default!;

    /// <summary>
    /// Number of files
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Total size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Formatted size
    /// </summary>
    public string FormattedSize { get; set; } = default!;
}
