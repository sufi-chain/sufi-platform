using System;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AIManagement.Storage;

/// <summary>
/// Abstraction for AI file storage - can use File-Manager or fallback to blob storage
/// </summary>
public interface IAIFileStorageService
{
    /// <summary>
    /// Upload a file and return the file ID and URL
    /// </summary>
    /// <param name="content">File content as byte array</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="mimeType">MIME type (e.g., "image/jpeg", "audio/mp3")</param>
    /// <param name="workspaceName">AI workspace name</param>
    /// <param name="capability">AI capability (e.g., "chat", "vision", "audio")</param>
    /// <param name="sourceEntityId">Source entity ID (e.g., chat message ID)</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>File storage result with ID and URL</returns>
    Task<FileStorageResult> UploadFileAsync(
        byte[] content,
        string fileName,
        string mimeType,
        string workspaceName,
        string capability,
        Guid? sourceEntityId = null,
        object? metadata = null);

    /// <summary>
    /// Delete a file by ID
    /// </summary>
    Task DeleteFileAsync(Guid fileId);

    /// <summary>
    /// Get the URL for accessing a file
    /// </summary>
    Task<string> GetFileUrlAsync(Guid fileId);

    /// <summary>
    /// Get file content for download
    /// </summary>
    Task<byte[]> GetFileContentAsync(Guid fileId);

    /// <summary>
    /// Check if File-Manager integration is available
    /// </summary>
    bool IsFileManagerAvailable { get; }
}

/// <summary>
/// Result of file upload operation
/// </summary>
public class FileStorageResult
{
    /// <summary>
    /// File ID (from File-Manager or generated)
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// URL to access the file
    /// </summary>
    public string FileUrl { get; set; } = default!;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Storage location (e.g., "FileManager", "BlobStorage")
    /// </summary>
    public string StorageLocation { get; set; } = default!;

    /// <summary>
    /// Full blob path (for reference)
    /// </summary>
    public string? BlobPath { get; set; }
}
