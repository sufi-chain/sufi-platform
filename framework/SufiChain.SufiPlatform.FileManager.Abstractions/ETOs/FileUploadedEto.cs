using SufiChain.SufiPlatform.EventBus;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Published when a file is uploaded and persisted.
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileUploaded")]
public class FileUploadedEto : SufiIntegrationEto
{
    /// <summary>Directory path where the file is stored.</summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>Generated storage file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Original file name from the user.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>MIME type.</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Size in bytes.</summary>
    public long SizeInBytes { get; set; }

    /// <summary>Uploader user id.</summary>
    public Guid? UploadedBy { get; set; }

    /// <summary>Upload timestamp (legacy; prefer <see cref="SufiIntegrationEto.OccurredAt"/>).</summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>File structure key.</summary>
    public string? StructureKey { get; set; }

    /// <summary>Source entity id.</summary>
    public Guid? SourceEntityId { get; set; }

    /// <summary>Source entity type.</summary>
    public string? SourceEntityType { get; set; }

    /// <summary>Custom metadata.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>Blob storage path.</summary>
    public string BlobName { get; set; } = string.Empty;

    /// <summary>File type label (Image, Video, …).</summary>
    public string FileType { get; set; } = string.Empty;
}
