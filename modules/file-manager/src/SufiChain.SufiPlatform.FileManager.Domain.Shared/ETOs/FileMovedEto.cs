using System;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.ETOs;

/// <summary>
/// Event Transfer Object published when a file is moved or renamed
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.FileManager.FileMoved")]
public class FileMovedEto : IMultiTenant
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
    /// Old file name
    /// </summary>
    public string OldFileName { get; set; } = default!;
    
    /// <summary>
    /// New file name
    /// </summary>
    public string NewFileName { get; set; } = default!;
    
    /// <summary>
    /// Old directory path
    /// </summary>
    public string OldDirectoryPath { get; set; } = default!;
    
    /// <summary>
    /// New directory path
    /// </summary>
    public string NewDirectoryPath { get; set; } = default!;
    
    /// <summary>
    /// Old blob storage path
    /// </summary>
    public string OldBlobName { get; set; } = default!;
    
    /// <summary>
    /// New blob storage path
    /// </summary>
    public string NewBlobName { get; set; } = default!;
    
    /// <summary>
    /// User ID who moved the file
    /// </summary>
    public Guid? MovedBy { get; set; }
    
    /// <summary>
    /// Move timestamp
    /// </summary>
    public DateTime MovedAt { get; set; }
}
