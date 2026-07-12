using System;
using System.Collections.Generic;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Event Transfer Object published when file metadata is updated
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileMetadataUpdated")]
public class FileMetadataUpdatedEto : IMultiTenant
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
    /// Updated metadata as key-value pairs
    /// </summary>
    public Dictionary<string, string> UpdatedMetadata { get; set; } = new();
    
    /// <summary>
    /// User ID who updated the metadata
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// Update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// File structure key (e.g., "AI.ProcessedAudio", "General")
    /// </summary>
    public string? StructureKey { get; set; }
    
    /// <summary>
    /// Source entity ID
    /// </summary>
    public Guid? SourceEntityId { get; set; }
}
