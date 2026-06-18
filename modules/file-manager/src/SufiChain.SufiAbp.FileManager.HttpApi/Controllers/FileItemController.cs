using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.FileManager.Controllers;

[Area("sabp-file-manager")]
[RemoteService(Name = "FileManager")]
[Route("api/file-manager/file-items")]
public class FileItemController : SufiAbpControllerBase, IFileItemAppService
{
    private readonly IFileItemAppService _fileItemAppService;
    private readonly FileManagerOptions _options;
    private readonly ILogger<FileItemController> _logger;

    // Default threshold: 10MB - files larger than this use streaming to avoid memory pressure
    private const long DefaultStreamingThresholdBytes = 10 * 1024 * 1024;

    public FileItemController(
        IFileItemAppService fileItemAppService,
        IOptions<FileManagerOptions> options,
        ILogger<FileItemController> logger)
    {
        _fileItemAppService = fileItemAppService;
        _options = options.Value;
        _logger = logger;
    }

    [NonAction]
    public Task<UploadValidationResult> ValidateUploadAsync(string fileName, string mimeType, string? structureKey, long fileSize)
    {
        return _fileItemAppService.ValidateUploadAsync(fileName, mimeType, structureKey, fileSize);
    }

    [HttpPost]
    [Route("upload-json")]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB limit
    [IgnoreAntiforgeryToken]
    public virtual Task<FileItemDto> UploadAsync([FromBody] UploadFileInput input)
    {
        return _fileItemAppService.UploadAsync(input);
    }

    [HttpGet]
    [Route("upload")]
    [IgnoreAntiforgeryToken]
    public virtual IActionResult UploadGetNotAllowed()
    {
        return StatusCode(405, new
        {
            error = new
            {
                message = "File upload endpoint requires POST multipart/form-data. The Blazor uploader sends this automatically after selecting a file."
            }
        });
    }

    [HttpPost]
    [Route("upload")]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [IgnoreAntiforgeryToken] // XHR uploads use bearer token auth, not cookies with antiforgery
    public virtual async Task<IActionResult> UploadFromFormAsync([FromForm] UploadFileFormRequest request)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
            return BadRequest(new { error = new { message = "No file provided" } });

        var maxFileSizeBytes = (long)_options.MaxUploadFileSizeMB * 1024 * 1024;
        if (file.Length > maxFileSizeBytes)
            return BadRequest(new { error = new { message = $"File size exceeds maximum allowed size of {_options.MaxUploadFileSizeMB}MB" } });

        try
        {
            // Validate mime/extension/size against structure without throwing (avoids app service exception)
            var validation = await _fileItemAppService.ValidateUploadAsync(file.FileName, file.ContentType ?? "", request.StructureKey, file.Length);
            if (!validation.IsValid)
                return BadRequest(new { error = new { message = validation.ErrorMessage } });

            // Determine threshold for streaming vs in-memory processing
            var streamingThreshold = (long)_options.MaxInMemoryFileSizeMB * 1024 * 1024;
            if (streamingThreshold <= 0)
            {
                streamingThreshold = DefaultStreamingThresholdBytes;
            }

            // For large files, use streaming upload (memory efficient)
            if (file.Length > streamingThreshold)
            {
                var streamInput = new UploadFileStreamInput
                {
                    FileName = file.FileName,
                    ContentStream = file.OpenReadStream(),
                    ContentLength = file.Length,
                    MimeType = file.ContentType,
                    StructureKey = request.StructureKey,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    FolderId = request.FolderId,
                    FolderPath = request.FolderPath,
                    AutoConfirm = request.AutoConfirm,
                    Alt = request.Alt,
                    SkipProcessing = request.SkipProcessing
                };

                var result = await _fileItemAppService.UploadStreamAsync(streamInput);
                return new JsonResult(result);
            }

            // For smaller files, use in-memory processing (allows image/video processing)
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream((int)file.Length); // Pre-allocate capacity
            await stream.CopyToAsync(memoryStream);
            var buffer = memoryStream.ToArray();

            var uploadInput = new UploadFileInput
            {
                FileName = file.FileName,
                Content = buffer,
                MimeType = file.ContentType,
                StructureKey = request.StructureKey,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                FolderId = request.FolderId,
                FolderPath = request.FolderPath,
                AutoConfirm = request.AutoConfirm,
                Alt = request.Alt
            };

            var dto = await _fileItemAppService.UploadAsync(uploadInput);
            return new JsonResult(dto);
        }
        catch (UserFriendlyException ex)
        {
            return BadRequest(new { error = new { message = ex.Message } });
        }
        catch (Exception ex)
        {
            return CreateUploadErrorResult(ex, file.FileName, file.Length, request.StructureKey);
        }
    }

    [HttpPost]
    [Route("upload-stream")]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [IgnoreAntiforgeryToken] // XHR uploads use bearer token auth, not cookies with antiforgery
    public virtual async Task<IActionResult> UploadStreamAsync([FromForm] UploadFileFormRequest request)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
            return BadRequest(new { error = new { message = "No file provided" } });

        var maxFileSizeBytes = (long)_options.MaxUploadFileSizeMB * 1024 * 1024;
        if (file.Length > maxFileSizeBytes)
            return BadRequest(new { error = new { message = $"File size exceeds maximum allowed size of {_options.MaxUploadFileSizeMB}MB" } });

        try
        {
            var validation = await _fileItemAppService.ValidateUploadAsync(file.FileName, file.ContentType ?? "", request.StructureKey, file.Length);
            if (!validation.IsValid)
                return BadRequest(new { error = new { message = validation.ErrorMessage } });

            // Always use streaming for this endpoint
            var streamInput = new UploadFileStreamInput
            {
                FileName = file.FileName,
                ContentStream = file.OpenReadStream(),
                ContentLength = file.Length,
                MimeType = file.ContentType,
                StructureKey = request.StructureKey,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                FolderId = request.FolderId,
                FolderPath = request.FolderPath,
                AutoConfirm = request.AutoConfirm,
                Alt = request.Alt,
                SkipProcessing = request.SkipProcessing
            };

            var result = await _fileItemAppService.UploadStreamAsync(streamInput);
            return new JsonResult(result);
        }
        catch (UserFriendlyException ex)
        {
            return BadRequest(new { error = new { message = ex.Message } });
        }
        catch (Exception ex)
        {
            return CreateUploadErrorResult(ex, file.FileName, file.Length, request.StructureKey);
        }
    }

    private IActionResult CreateUploadErrorResult(Exception exception, string fileName, long fileLength, string? structureKey)
    {
        var rootException = exception.GetBaseException();
        _logger.LogError(exception,
            "File upload failed. FileName: {FileName}, Size: {Size}, StructureKey: {StructureKey}",
            fileName,
            fileLength,
            structureKey);

        return StatusCode(500, new
        {
            error = new
            {
                message = $"File upload failed: {rootException.Message}",
                details = rootException.GetType().FullName
            }
        });
    }

    [HttpPost]
    [Route("upload-multiple-json")]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB limit
    [IgnoreAntiforgeryToken]
    public virtual Task<ListResultDto<FileItemDto>> UploadMultipleAsync([FromBody] UploadMultipleFileInput input)
    {
        return _fileItemAppService.UploadMultipleAsync(input);
    }

    [HttpPost]
    [Route("upload-multiple")]
    [RequestSizeLimit(500 * 1024 * 1024)] // 500MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [IgnoreAntiforgeryToken] // XHR uploads use bearer token auth, not cookies with antiforgery
    public virtual async Task<ListResultDto<FileItemDto>> UploadMultipleFromFormAsync([FromForm] UploadMultipleFilesFormRequest request)
    {
        var files = request.Files;
        if (files == null || files.Count == 0)
        {
            throw new UserFriendlyException("No files provided");
        }

        var results = new List<FileItemDto>();
        var streamingThreshold = (long)_options.MaxInMemoryFileSizeMB * 1024 * 1024;
        if (streamingThreshold <= 0)
        {
            streamingThreshold = DefaultStreamingThresholdBytes;
        }

        foreach (var file in files)
        {
            // For large files, use streaming
            if (file.Length > streamingThreshold)
            {
                var streamInput = new UploadFileStreamInput
                {
                    FileName = file.FileName,
                    ContentStream = file.OpenReadStream(),
                    ContentLength = file.Length,
                    MimeType = file.ContentType,
                    StructureKey = request.StructureKey,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    FolderId = request.FolderId,
                    FolderPath = request.FolderPath,
                    AutoConfirm = request.AutoConfirm,
                    Alt = request.Alt,
                    SkipProcessing = true // Skip processing for large files in batch upload
                };

                var result = await _fileItemAppService.UploadStreamAsync(streamInput);
                results.Add(result);
            }
            else
            {
                // For smaller files, use in-memory processing
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream((int)file.Length);
                await stream.CopyToAsync(memoryStream);
                var buffer = memoryStream.ToArray();

                var singleInput = new UploadFileInput
                {
                    FileName = file.FileName,
                    Content = buffer,
                    MimeType = file.ContentType,
                    StructureKey = request.StructureKey,
                    EntityType = request.EntityType,
                    EntityId = request.EntityId,
                    FolderId = request.FolderId,
                    FolderPath = request.FolderPath,
                    AutoConfirm = request.AutoConfirm,
                    Alt = request.Alt
                };

                var result = await _fileItemAppService.UploadAsync(singleInput);
                results.Add(result);
            }
        }

        return new ListResultDto<FileItemDto>(results);
    }

    [HttpGet]
    [Route("{id:guid}")]
    public virtual Task<FileItemDto> GetAsync(Guid id)
    {
        return _fileItemAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<FileItemDto>> GetListAsync([FromQuery] GetFileListInput input)
    {
        return _fileItemAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id:guid}/download")]
    [AllowAnonymous]
    public virtual async Task<IActionResult> DownloadAsync(Guid id, [FromQuery] string? token = null)
    {
        var result = await _fileItemAppService.GetDownloadContentAsync(id, token);
        if (result.IsForbidden)
            return Forbid();
        if (result.Content == null)
            return NotFound();
        return File(result.Content.Content, result.Content.MimeType, result.Content.FileName);
    }

    [HttpGet]
    [Route("{id:guid}/stream")]
    [AllowAnonymous]
    public virtual async Task<IActionResult> StreamAsync(Guid id, [FromQuery] string? token = null)
    {
        var result = await _fileItemAppService.GetStreamContentAsync(id, token);
        if (result.IsForbidden)
            return Forbid();
        if (result.Content == null)
            return NotFound();
        return File(result.Content.Stream, result.Content.MimeType, enableRangeProcessing: true);
    }

    [HttpGet]
    [Route("{id:guid}/thumbnail")]
    [AllowAnonymous]
    public virtual async Task<IActionResult> GetThumbnailAsync(Guid id, [FromQuery] string? token = null)
    {
        var result = await _fileItemAppService.GetThumbnailContentAsync(id, token);
        if (result.IsForbidden)
            return Forbid();
        if (result.Content == null)
            return NotFound("Thumbnail not available");
        return File(result.Content.Content, result.Content.MimeType);
    }

    [HttpPut]
    [Route("{id:guid}/content")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit for edited images
    [IgnoreAntiforgeryToken]
    public virtual async Task<FileItemDto> ReplaceContentAsync(Guid id, [FromBody] ReplaceFileContentInput input)
    {
        if (input?.Content == null || input.Content.Length == 0)
        {
            throw new UserFriendlyException("Content is required");
        }
        return await _fileItemAppService.ReplaceContentAsync(id, input);
    }

    [HttpPost]
    [Route("save-as/{sourceId:guid}")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [IgnoreAntiforgeryToken]
    public virtual async Task<FileItemDto> SaveAsAsync(Guid sourceId, [FromBody] SaveAsFileInput input)
    {
        if (input?.Content == null || input.Content.Length == 0)
        {
            throw new UserFriendlyException("Content is required");
        }
        if (input.SourceId != sourceId)
        {
            throw new UserFriendlyException("SourceId in body must match the route.");
        }
        return await _fileItemAppService.SaveAsAsync(sourceId, input);
    }

    [HttpPut]
    [Route("{id:guid}/metadata")]
    public virtual Task<FileItemDto> UpdateMetadataAsync(Guid id, UpdateFileMetadataInput input)
    {
        return _fileItemAppService.UpdateMetadataAsync(id, input);
    }

    [HttpPost]
    [Route("{id:guid}/confirm")]
    public virtual Task<FileItemDto> ConfirmAsync(Guid id)
    {
        return _fileItemAppService.ConfirmAsync(id);
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _fileItemAppService.DeleteAsync(id);
    }

    [HttpPost]
    [Route("delete-many")]
    public virtual Task DeleteManyAsync([FromBody] Guid[] ids)
    {
        return _fileItemAppService.DeleteManyAsync(ids);
    }

    [HttpGet]
    [Route("storage-quota")]
    public virtual Task<StorageQuotaDto> GetStorageQuotaAsync()
    {
        return _fileItemAppService.GetStorageQuotaAsync();
    }

    [HttpGet]
    [Route("statistics")]
    public virtual Task<FileStatisticsDto> GetStatisticsAsync()
    {
        return _fileItemAppService.GetStatisticsAsync();
    }

    [HttpGet]
    [Route("{id:guid}/download-url")]
    public virtual Task<string> GetDownloadUrlAsync(Guid id)
    {
        return _fileItemAppService.GetDownloadUrlAsync(id);
    }

    [HttpGet]
    [Route("{id:guid}/thumbnail-url")]
    public virtual Task<string> GetThumbnailUrlAsync(Guid id)
    {
        return _fileItemAppService.GetThumbnailUrlAsync(id);
    }

    [HttpGet]
    [Route("{id:guid}/temporary-access-url")]
    public virtual Task<string> GetTemporaryAccessUrlAsync(Guid id, [FromQuery] int durationMinutes = 1440)
    {
        return _fileItemAppService.GetTemporaryAccessUrlAsync(id, durationMinutes);
    }


    [NonAction]
    Task<FileItemDto> IFileItemAppService.UploadStreamAsync(UploadFileStreamInput input)
        => _fileItemAppService.UploadStreamAsync(input);

    [NonAction]
    Task<FileContentResultDto> IFileItemAppService.GetDownloadContentAsync(Guid id, string? token)
        => _fileItemAppService.GetDownloadContentAsync(id, token);

    [NonAction]
    Task<StreamContentResultDto> IFileItemAppService.GetStreamContentAsync(Guid id, string? token)
        => _fileItemAppService.GetStreamContentAsync(id, token);

    [NonAction]
    Task<FileContentResultDto> IFileItemAppService.GetThumbnailContentAsync(Guid id, string? token)
        => _fileItemAppService.GetThumbnailContentAsync(id, token);

    [HttpPost]
    [Route("{id:guid}/archive")]
    public virtual Task ArchiveAsync(Guid id, [FromQuery] string? reason = null)
    {
        return _fileItemAppService.ArchiveAsync(id, reason);
    }

    [HttpPost]
    [Route("archive-old")]
    public virtual Task<int> ArchiveOldFilesAsync([FromBody] ArchiveOldFilesInput input)
    {
        return _fileItemAppService.ArchiveOldFilesAsync(input);
    }

    [HttpPost]
    [Route("{id:guid}/restore")]
    public virtual Task RestoreFromArchiveAsync(Guid id)
    {
        return _fileItemAppService.RestoreFromArchiveAsync(id);
    }

    [HttpGet]
    [Route("archived")]
    public virtual Task<PagedResultDto<FileItemDto>> GetArchivedFilesAsync([FromQuery] GetArchivedFilesInput input)
    {
        return _fileItemAppService.GetArchivedFilesAsync(input);
    }

}
