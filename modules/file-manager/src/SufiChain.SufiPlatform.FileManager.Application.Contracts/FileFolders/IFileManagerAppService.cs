using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Application service for file manager operations (clipboard, bulk operations)
/// </summary>
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
public interface IFileManagerAppService : IApplicationService
{
    #region Clipboard Operations

    /// <summary>
    /// Cut files/folders to clipboard (prepare for move)
    /// </summary>
    Task<ClipboardResultDto> CutAsync(ClipboardOperationInput input);

    /// <summary>
    /// Copy files/folders to clipboard (prepare for copy)
    /// </summary>
    Task<ClipboardResultDto> CopyAsync(ClipboardOperationInput input);

    /// <summary>
    /// Paste clipboard contents to target folder
    /// </summary>
    Task<PasteResultDto> PasteAsync(PasteInput input);

    /// <summary>
    /// Get current clipboard state
    /// </summary>
    Task<ClipboardStateDto> GetClipboardStateAsync();

    /// <summary>
    /// Clear clipboard
    /// </summary>
    Task ClearClipboardAsync();

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Move multiple items to a folder
    /// </summary>
    Task<BulkOperationResultDto> MoveItemsAsync(BulkMoveInput input);

    /// <summary>
    /// Copy multiple items to a folder
    /// </summary>
    Task<BulkOperationResultDto> CopyItemsAsync(BulkCopyInput input);

    /// <summary>
    /// Delete multiple items
    /// </summary>
    Task<BulkOperationResultDto> DeleteItemsAsync(BulkDeleteInput input);

    /// <summary>
    /// Download multiple items as ZIP
    /// </summary>
    Task<DownloadResultDto> DownloadAsZipAsync(DownloadInput input);

    /// <summary>
    /// Retrieves a cached ZIP download by token.
    /// </summary>
    Task<ZipDownloadContentDto?> GetZipDownloadAsync(string token);

    #endregion

    #region Search

    /// <summary>
    /// Search files and folders
    /// </summary>
    Task<SearchResultDto> SearchAsync(SearchInput input);

    #endregion
}

#region Input DTOs

/// <summary>
/// Input for clipboard operations
/// </summary>
public class ClipboardOperationInput
{
    /// <summary>
    /// File IDs to cut/copy
    /// </summary>
    public List<Guid> FileIds { get; set; } = new();

    /// <summary>
    /// Folder IDs to cut/copy
    /// </summary>
    public List<Guid> FolderIds { get; set; } = new();
}

/// <summary>
/// Input for paste operation
/// </summary>
public class PasteInput
{
    /// <summary>
    /// Target folder ID
    /// </summary>
    public Guid? TargetFolderId { get; set; }

    /// <summary>
    /// Target virtual path (alternative to FolderId)
    /// </summary>
    public string? TargetPath { get; set; }

    /// <summary>
    /// How to handle name conflicts
    /// </summary>
    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.Rename;
}

/// <summary>
/// Input for bulk move operation
/// </summary>
public class BulkMoveInput
{
    public List<Guid> FileIds { get; set; } = new();
    public List<Guid> FolderIds { get; set; } = new();
    public Guid? TargetFolderId { get; set; }
    public string? TargetPath { get; set; }
}

/// <summary>
/// Input for bulk copy operation
/// </summary>
public class BulkCopyInput
{
    public List<Guid> FileIds { get; set; } = new();
    public List<Guid> FolderIds { get; set; } = new();
    public Guid? TargetFolderId { get; set; }
    public string? TargetPath { get; set; }
}

/// <summary>
/// Input for bulk delete operation
/// </summary>
public class BulkDeleteInput
{
    public List<Guid> FileIds { get; set; } = new();
    public List<Guid> FolderIds { get; set; } = new();
    public bool Permanent { get; set; } = false;
}

/// <summary>
/// Input for download operation
/// </summary>
public class DownloadInput
{
    public List<Guid> FileIds { get; set; } = new();
    public List<Guid> FolderIds { get; set; } = new();
}

/// <summary>
/// Input for search operation
/// </summary>
public class SearchInput
{
    /// <summary>
    /// Search query
    /// </summary>
    public string Query { get; set; } = default!;

    /// <summary>
    /// Folder to search in (null for all)
    /// </summary>
    public Guid? ScopeFolderId { get; set; }

    /// <summary>
    /// Virtual path to search in
    /// </summary>
    public string? ScopePath { get; set; }

    /// <summary>
    /// Include subfolders in search
    /// </summary>
    public bool IncludeSubfolders { get; set; } = true;

    /// <summary>
    /// Search in file names
    /// </summary>
    public bool SearchFileNames { get; set; } = true;

    /// <summary>
    /// Search in folder names
    /// </summary>
    public bool SearchFolderNames { get; set; } = true;

    /// <summary>
    /// Filter by file type
    /// </summary>
    public string? FileTypeFilter { get; set; }

    /// <summary>
    /// Skip count for pagination
    /// </summary>
    public int SkipCount { get; set; }

    /// <summary>
    /// Max results
    /// </summary>
    public int MaxResultCount { get; set; } = 50;
}

#endregion

#region Result DTOs

/// <summary>
/// Conflict resolution strategy
/// </summary>
public enum ConflictResolution
{
    /// <summary>
    /// Skip conflicting items
    /// </summary>
    Skip,

    /// <summary>
    /// Overwrite existing items
    /// </summary>
    Overwrite,

    /// <summary>
    /// Rename new items
    /// </summary>
    Rename
}

/// <summary>
/// Result of clipboard operation
/// </summary>
public class ClipboardResultDto
{
    public bool Success { get; set; }
    public int FileCount { get; set; }
    public int FolderCount { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Current clipboard state
/// </summary>
public class ClipboardStateDto
{
    public bool HasContent { get; set; }
    public ClipboardOperation Operation { get; set; }
    public List<Guid> FileIds { get; set; } = new();
    public List<Guid> FolderIds { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Type of clipboard operation
/// </summary>
public enum ClipboardOperation
{
    None,
    Cut,
    Copy
}

/// <summary>
/// Result of paste operation
/// </summary>
public class PasteResultDto
{
    public bool Success { get; set; }
    public int FilesCopied { get; set; }
    public int FilesMoved { get; set; }
    public int FoldersCopied { get; set; }
    public int FoldersMoved { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of bulk operation
/// </summary>
public class BulkOperationResultDto
{
    public bool Success { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<BulkOperationErrorDto> Errors { get; set; } = new();
}

/// <summary>
/// Error detail for bulk operation
/// </summary>
public class BulkOperationErrorDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = default!;
    public string ErrorMessage { get; set; } = default!;
}

/// <summary>
/// Result of download operation
/// </summary>
public class DownloadResultDto
{
    public bool Success { get; set; }
    public string? DownloadUrl { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public int FileCount { get; set; }
}

/// <summary>
/// Result of search operation
/// </summary>
public class SearchResultDto
{
    public List<SearchResultItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public string Query { get; set; } = default!;
}

/// <summary>
/// Individual search result item
/// </summary>
public class SearchResultItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public bool IsFolder { get; set; }
    public string? Icon { get; set; }
    public string? MimeType { get; set; }
    public long? Size { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public Guid? ParentFolderId { get; set; }
    public string? HighlightedName { get; set; }
}

#endregion
