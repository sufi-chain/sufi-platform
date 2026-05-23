using System;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.FileManager.ETOs;

/// <summary>
/// Event Transfer Object published when a file is deleted
/// </summary>
[Serializable]
[EventName("SufiChain.SufiAbp.FileManager.FileDeleted")]
public class FileDeletedEto : IMultiTenant
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
    /// File name that was deleted
    /// </summary>
    public string FileName { get; set; } = default!;
    
    /// <summary>
    /// Directory path where file was stored
    /// </summary>
    public string DirectoryPath { get; set; } = default!;
    
    /// <summary>
    /// Blob storage path
    /// </summary>
    public string BlobName { get; set; } = default!;
    
    /// <summary>
    /// User ID who deleted the file
    /// </summary>
    public Guid? DeletedBy { get; set; }
    
    /// <summary>
    /// Deletion timestamp
    /// </summary>
    public DateTime DeletedAt { get; set; }
    
    /// <summary>
    /// File structure key (e.g., "AIManagement.ProcessedAudio", "General")
    /// </summary>
    public string? StructureKey { get; set; }
    
    /// <summary>
    /// Source entity ID
    /// </summary>
    public Guid? SourceEntityId { get; set; }
}
