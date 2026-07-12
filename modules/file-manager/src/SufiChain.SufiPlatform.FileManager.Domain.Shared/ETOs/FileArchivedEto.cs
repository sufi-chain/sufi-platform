using System;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Event Transfer Object published when a file is archived
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileArchived")]
public class FileArchivedEto : IMultiTenant
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
    /// File name
    /// </summary>
    public string FileName { get; set; } = default!;
    
    /// <summary>
    /// Original directory path (before archiving)
    /// </summary>
    public string OriginalDirectoryPath { get; set; } = default!;
    
    /// <summary>
    /// Archive directory path
    /// </summary>
    public string ArchiveDirectoryPath { get; set; } = default!;
    
    /// <summary>
    /// Blob storage path
    /// </summary>
    public string BlobName { get; set; } = default!;
    
    /// <summary>
    /// User ID who archived the file (null for automatic archiving)
    /// </summary>
    public Guid? ArchivedBy { get; set; }
    
    /// <summary>
    /// Archive timestamp
    /// </summary>
    public DateTime ArchivedAt { get; set; }
    
    /// <summary>
    /// Reason for archiving (e.g., "Retention policy", "Manual archive")
    /// </summary>
    public string? ArchiveReason { get; set; }
    
    /// <summary>
    /// File structure key (e.g., "AI.ProcessedAudio", "General")
    /// </summary>
    public string? StructureKey { get; set; }
}
