using System;
using System.Collections.Generic;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Event Transfer Object published when a file is uploaded
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileUploaded")]
public class FileUploadedEto : IMultiTenant
{
    /// <summary>
    /// File ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Tenant ID for multi-tenancy support
    /// </summary>
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// Directory path where file is stored
    /// </summary>
    public string DirectoryPath { get; set; } = default!;
    
    /// <summary>
    /// File name (generated)
    /// </summary>
    public string FileName { get; set; } = default!;
    
    /// <summary>
    /// Original file name from user
    /// </summary>
    public string OriginalFileName { get; set; } = default!;
    
    /// <summary>
    /// MIME type (e.g., "image/jpeg", "audio/mp3")
    /// </summary>
    public string MimeType { get; set; } = default!;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeInBytes { get; set; }
    
    /// <summary>
    /// User ID who uploaded the file
    /// </summary>
    public Guid? UploadedBy { get; set; }
    
    /// <summary>
    /// Upload timestamp
    /// </summary>
    public DateTime UploadedAt { get; set; }
    
    /// <summary>
    /// File structure key (e.g., "AI.ProcessedAudio", "General")
    /// </summary>
    public string? StructureKey { get; set; }
    
    /// <summary>
    /// Source entity ID (e.g., chat message ID, vision request ID)
    /// </summary>
    public Guid? SourceEntityId { get; set; }
    
    /// <summary>
    /// Source entity type (e.g., "ChatMessage", "VisionRequest", "AudioTranscription")
    /// </summary>
    public string? SourceEntityType { get; set; }
    
    /// <summary>
    /// Custom metadata as key-value pairs
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    /// <summary>
    /// Blob storage path
    /// </summary>
    public string BlobName { get; set; } = default!;
    
    /// <summary>
    /// File type (Image, Video, Audio, Document, etc.)
    /// </summary>
    public string FileType { get; set; } = default!;
}
