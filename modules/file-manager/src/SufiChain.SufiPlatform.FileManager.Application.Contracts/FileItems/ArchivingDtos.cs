using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Input for archiving old files
/// </summary>
public class ArchiveOldFilesInput
{
    /// <summary>
    /// Directory path to archive files from (optional, null = all directories)
    /// </summary>
    public string? DirectoryPath { get; set; }

    /// <summary>
    /// Archive files older than this many days
    /// </summary>
    public int OlderThanDays { get; set; } = 90;

    /// <summary>
    /// File structure key filter (optional, e.g., "General")
    /// </summary>
    public string? StructureKey { get; set; }

    /// <summary>
    /// Reason for archiving
    /// </summary>
    public string? ArchiveReason { get; set; }

    /// <summary>
    /// Maximum number of files to archive in this operation
    /// </summary>
    public int MaxFiles { get; set; } = 1000;
}

/// <summary>
/// Input for getting archived files
/// </summary>
public class GetArchivedFilesInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Filter by file structure key
    /// </summary>
    public string? StructureKey { get; set; }

    /// <summary>
    /// Filter by directory path
    /// </summary>
    public string? DirectoryPath { get; set; }

    /// <summary>
    /// Filter by file name
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Filter by archived date range - start
    /// </summary>
    public DateTime? ArchivedAfter { get; set; }

    /// <summary>
    /// Filter by archived date range - end
    /// </summary>
    public DateTime? ArchivedBefore { get; set; }
}
