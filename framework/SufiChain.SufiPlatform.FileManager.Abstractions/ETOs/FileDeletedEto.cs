using SufiChain.SufiPlatform.EventBus;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Published when a file is deleted.
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileDeleted")]
public class FileDeletedEto : SufiIntegrationEto
{
    /// <summary>Deleted file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Directory path where the file was stored.</summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>Blob storage path.</summary>
    public string BlobName { get; set; } = string.Empty;

    /// <summary>User who deleted the file.</summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>Deletion timestamp (legacy; prefer <see cref="SufiIntegrationEto.OccurredAt"/>).</summary>
    public DateTime DeletedAt { get; set; }

    /// <summary>File structure key.</summary>
    public string? StructureKey { get; set; }

    /// <summary>Source entity id.</summary>
    public Guid? SourceEntityId { get; set; }
}
