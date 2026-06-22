using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Storage;

/// <summary>
/// Fallback file storage service that uses blob storage directly (when File-Manager is not available)
/// </summary>
public class DefaultBlobStorageService : IAIFileStorageService, ITransientDependency
{
    private readonly IBlobContainer _blobContainer;
    private readonly ILogger<DefaultBlobStorageService> _logger;

    public DefaultBlobStorageService(
        IBlobContainer blobContainer,
        ILogger<DefaultBlobStorageService> logger)
    {
        _blobContainer = blobContainer;
        _logger = logger;
    }

    public bool IsFileManagerAvailable => false;

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
            "Uploading file to blob storage (File-Manager not available): {FileName}, Size: {Size} bytes",
            fileName,
            content.Length);

        // Generate blob path: ai/{workspace}/{capability}/{yyyy-MM}/{guid}.ext
        var fileId = Guid.NewGuid();
        var blobPath = GenerateBlobPath(workspaceName, capability, fileName, fileId);

        // Save to blob storage
        await _blobContainer.SaveAsync(blobPath, content, overrideExisting: true);

        _logger.LogInformation(
            "File uploaded successfully to blob storage. FileId: {FileId}, Path: {BlobPath}",
            fileId,
            blobPath);

        // Note: In blob storage mode, we don't have a proper URL generation mechanism
        // This would need to be configured based on your blob storage provider
        var fileUrl = $"/api/ai/files/{fileId}";

        return new FileStorageResult
        {
            FileId = fileId,
            FileUrl = fileUrl,
            SizeInBytes = content.Length,
            StorageLocation = "BlobStorage",
            BlobPath = blobPath
        };
    }

    public async Task DeleteFileAsync(Guid fileId)
    {
        _logger.LogInformation("Deleting file from blob storage: {FileId}", fileId);

        // Note: We need to reconstruct the blob path from the fileId
        // This is a limitation of the fallback approach
        // In a real implementation, you'd need to store the blob path mapping
        
        _logger.LogWarning(
            "Cannot delete file {FileId} - blob path mapping not available in fallback mode. " +
            "Consider using File-Manager for proper file management.",
            fileId);

        // For now, we'll just log a warning
        // A proper implementation would require a separate table to track blob paths
    }

    public async Task<string> GetFileUrlAsync(Guid fileId)
    {
        // Return a simple URL - actual implementation depends on your setup
        return $"/api/ai/files/{fileId}";
    }

    public async Task<byte[]> GetFileContentAsync(Guid fileId)
    {
        _logger.LogWarning(
            "Cannot retrieve file {FileId} - blob path mapping not available in fallback mode. " +
            "Consider using File-Manager for proper file management.",
            fileId);

        throw new Volo.Abp.UserFriendlyException(
            "File retrieval not supported in fallback blob storage mode. Please use File-Manager integration.");
    }

    private string GenerateBlobPath(string workspaceName, string capability, string fileName, Guid fileId)
    {
        var now = DateTime.UtcNow;
        var sanitizedWorkspace = SanitizePathSegment(workspaceName);
        var sanitizedCapability = SanitizePathSegment(capability);
        var extension = System.IO.Path.GetExtension(fileName);
        
        return $"ai/{sanitizedWorkspace}/{sanitizedCapability}/{now:yyyy-MM}/{fileId}{extension}";
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
}
