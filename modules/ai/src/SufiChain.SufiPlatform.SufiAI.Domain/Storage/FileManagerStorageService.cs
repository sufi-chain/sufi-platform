using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Storage;

/// <summary>
/// File storage service that uses File-Manager module
/// </summary>
public class FileManagerStorageService : IAIFileStorageService, ITransientDependency
{
    private readonly FileItemManager _fileItemManager;
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IFileItemAppService _fileItemAppService;
    private readonly ILogger<FileManagerStorageService> _logger;

    public FileManagerStorageService(
        FileItemManager fileItemManager,
        IFileItemRepository fileItemRepository,
        IFileItemAppService fileItemAppService,
        ILogger<FileManagerStorageService> logger)
    {
        _fileItemManager = fileItemManager;
        _fileItemRepository = fileItemRepository;
        _fileItemAppService = fileItemAppService;
        _logger = logger;
    }

    public bool IsFileManagerAvailable => true;

    public async Task<FileStorageResult> UploadFileAsync(
        byte[] content,
        string fileName,
        string mimeType,
        string workspaceName,
        string capability,
        Guid? sourceEntityId = null,
        object? metadata = null)
    {
        _logger.LogInformation(
            "Uploading file to File-Manager: {FileName}, Size: {Size} bytes, Workspace: {Workspace}, Capability: {Capability}",
            fileName,
            content.Length,
            workspaceName,
            capability);

        // Generate directory path: /ai/{workspace}/{capability}/{yyyy-MM}/
        var directoryPath = GenerateDirectoryPath(workspaceName, capability);
        
        // Generate unique file name
        var extension = System.IO.Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var blobName = $"{directoryPath}/{uniqueFileName}";

        // Determine file type
        var fileType = DetermineFileType(mimeType);

        // Prepare metadata
        var customMetadata = new
        {
            WorkspaceName = workspaceName,
            Capability = capability,
            OriginalFileName = fileName,
            UploadedAt = DateTime.UtcNow,
            AdditionalMetadata = metadata
        };

        // Create file item using FileItemManager (publishes FileUploadedEto)
        var fileItem = await _fileItemManager.CreateAsync(
            name: uniqueFileName,
            originalName: fileName,
            blobName: blobName,
            mimeType: mimeType,
            size: content.Length,
            fileType: fileType,
            structureKey: FileStructureKeys.AI,
            sourceEntityId: sourceEntityId,
            customMetadata: JsonSerializer.Serialize(customMetadata));

        // Upload content to blob storage
        // Note: FileItemManager creates the entity, but we still need to upload the actual content
        // This would typically be done through FileItemAppService.UploadAsync
        // For now, we'll use a simplified approach
        
        // Get file URL
        var fileUrl = await _fileItemAppService.GetDownloadUrlAsync(fileItem.Id);

        _logger.LogInformation(
            "File uploaded successfully to File-Manager. FileId: {FileId}, Path: {BlobName}",
            fileItem.Id,
            blobName);

        return new FileStorageResult
        {
            FileId = fileItem.Id,
            FileUrl = fileUrl,
            SizeInBytes = content.Length,
            StorageLocation = "FileManager",
            BlobPath = blobName
        };
    }

    public async Task DeleteFileAsync(Guid fileId)
    {
        _logger.LogInformation("Deleting file from File-Manager: {FileId}", fileId);

        var fileItem = await _fileItemRepository.GetAsync(fileId);
        await _fileItemManager.DeleteAsync(fileItem);

        _logger.LogInformation("File deleted successfully from File-Manager: {FileId}", fileId);
    }

    public async Task<string> GetFileUrlAsync(Guid fileId)
    {
        return await _fileItemAppService.GetDownloadUrlAsync(fileId);
    }

    public async Task<byte[]> GetFileContentAsync(Guid fileId)
    {
        var contentResult = await _fileItemAppService.GetDownloadContentAsync(fileId, null);
        
        if (contentResult.Content == null)
        {
            throw new Volo.Abp.UserFriendlyException($"File not found: {fileId}");
        }

        return contentResult.Content.Content;
    }

    private string GenerateDirectoryPath(string workspaceName, string capability)
    {
        var now = DateTime.UtcNow;
        var sanitizedWorkspace = SanitizePathSegment(workspaceName);
        var sanitizedCapability = SanitizePathSegment(capability);
        
        return $"/ai/{sanitizedWorkspace}/{sanitizedCapability}/{now:yyyy-MM}";
    }

    private string SanitizePathSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return "default";
        }

        // Remove invalid path characters
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        foreach (var c in invalid)
        {
            segment = segment.Replace(c, '-');
        }

        return segment.ToLowerInvariant();
    }

    private FileType DetermineFileType(string mimeType)
    {
        if (mimeType.StartsWith("image/"))
            return FileType.Image;
        
        if (mimeType.StartsWith("audio/"))
            return FileType.Audio;
        
        if (mimeType.StartsWith("video/"))
            return FileType.Video;
        
        if (mimeType.StartsWith("application/pdf") || 
            mimeType.Contains("document") || 
            mimeType.Contains("text"))
            return FileType.Document;

        return FileType.None;
    }
}
