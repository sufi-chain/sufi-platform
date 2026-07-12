using System;
using System.Threading.Tasks;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
public interface IFileItemAppService : IApplicationService
{
    /// <summary>
    /// Validates upload (mime type, extension, size) without throwing. Use before upload to return 400 with message.
    /// </summary>
    [RemoteService(false)]
    Task<UploadValidationResult> ValidateUploadAsync(string fileName, string mimeType, string? structureKey, long fileSize);

    /// <summary>
    /// Upload a single file (loads entire file into memory - suitable for small files)
    /// </summary>
    [RemoteService(false)]
    Task<FileItemDto> UploadAsync(UploadFileInput input);

    /// <summary>
    /// Upload a single file using streaming (memory efficient - suitable for large files)
    /// </summary>
    [RemoteService(false)]
    Task<FileItemDto> UploadStreamAsync(UploadFileStreamInput input);

    /// <summary>
    /// Upload multiple files
    /// </summary>
    [RemoteService(false)]
    Task<ListResultDto<FileItemDto>> UploadMultipleAsync(UploadMultipleFileInput input);

    /// <summary>
    /// Get a file item by ID
    /// </summary>
    Task<FileItemDto> GetAsync(Guid id);

    /// <summary>
    /// Get list of file items with filtering and paging
    /// </summary>
    Task<PagedResultDto<FileItemDto>> GetListAsync(GetFileListInput input);

    /// <summary>
    /// Delete a file item (soft delete)
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Update file metadata (alt text, tags)
    /// </summary>
    Task<FileItemDto> UpdateMetadataAsync(Guid id, UpdateFileMetadataInput input);

    /// <summary>
    /// Get file statistics for current tenant (counts by type, total size)
    /// </summary>
    Task<FileStatisticsDto> GetStatisticsAsync();

    /// <summary>
    /// Get storage quota for current tenant/user
    /// </summary>
    Task<StorageQuotaDto> GetStorageQuotaAsync();

    /// <summary>
    /// Get the download URL for a file item
    /// </summary>
    Task<string> GetDownloadUrlAsync(Guid id);

    /// <summary>
    /// Get the thumbnail URL for a file item
    /// </summary>
    Task<string> GetThumbnailUrlAsync(Guid id);

    /// <summary>
    /// Get a temporary access URL with configurable duration. For S3 private files, returns presigned URL.
    /// For other providers, returns standard token-based download URL.
    /// </summary>
    /// <param name="id">File item ID.</param>
    /// <param name="durationMinutes">Validity in minutes (1–10080 = 1 min–7 days).</param>
    Task<string> GetTemporaryAccessUrlAsync(Guid id, int durationMinutes);

    /// <summary>
    /// Confirm temporary uploaded file (move from temp to permanent storage)
    /// </summary>
    Task<FileItemDto> ConfirmAsync(Guid id);

    /// <summary>
    /// Bulk delete file items
    /// </summary>
    Task DeleteManyAsync(Guid[] ids);

    /// <summary>
    /// Replace file content (e.g., after frontend image editing).
    /// Backend stores bytes as-is; regenerates thumbnail for images.
    /// </summary>
    Task<FileItemDto> ReplaceContentAsync(Guid id, ReplaceFileContentInput input);

    /// <summary>
    /// Save As - create a new file from edited content (e.g., from image editor).
    /// Client sends the final edited bytes; backend creates a new file in the specified folder.
    /// </summary>
    Task<FileItemDto> SaveAsAsync(Guid sourceId, SaveAsFileInput input);

    /// <summary>
    /// Get file content for download. Handles token-based, public, and authenticated access.
    /// Use IsForbidden for 403; null Content for 404.
    /// </summary>
    [RemoteService(false)]
    Task<FileContentResultDto> GetDownloadContentAsync(Guid id, string? token);

    /// <summary>
    /// Get file stream for streaming (e.g. video with range support). Caller must dispose stream.
    /// Handles token-based, public, and authenticated access. Use IsForbidden for 403; null Content for 404.
    /// </summary>
    [RemoteService(false)]
    Task<StreamContentResultDto> GetStreamContentAsync(Guid id, string? token);

    /// <summary>
    /// Get thumbnail content. Use IsForbidden for 403; null Content for 404 or thumbnail unavailable.
    /// Handles token-based, public, and authenticated access.
    /// </summary>
    [RemoteService(false)]
    Task<FileContentResultDto> GetThumbnailContentAsync(Guid id, string? token);

    /// <summary>
    /// Archive a file item
    /// </summary>
    Task ArchiveAsync(Guid id, string? reason = null);

    /// <summary>
    /// Archive multiple files based on criteria (returns count of archived files)
    /// </summary>
    Task<int> ArchiveOldFilesAsync(ArchiveOldFilesInput input);

    /// <summary>
    /// Restore a file from archive
    /// </summary>
    Task RestoreFromArchiveAsync(Guid id);

    /// <summary>
    /// Get list of archived files
    /// </summary>
    Task<PagedResultDto<FileItemDto>> GetArchivedFilesAsync(GetArchivedFilesInput input);
}
