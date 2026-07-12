using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Represents a file (image, video, document, etc.)
/// </summary>
public class FileItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// Tenant ID for multi-tenancy support
    /// </summary>
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// Generated filename (e.g., "guid.jpg")
    /// </summary>
    public string Name { get; set; } = default!;
    
    /// <summary>
    /// Original filename from user
    /// </summary>
    public string OriginalName { get; set; } = default!;
    
    /// <summary>
    /// Full storage path in blob container
    /// </summary>
    public string BlobName { get; set; } = default!;
    
    /// <summary>
    /// MIME type (e.g., "image/jpeg", "video/mp4")
    /// </summary>
    public string MimeType { get; set; } = default!;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }
    
    /// <summary>
    /// Type of file content
    /// </summary>
    public FileType FileType { get; set; }
    
    /// <summary>
    /// Image/Video width in pixels
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// Image/Video height in pixels
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// Video duration
    /// </summary>
    public TimeSpan? Duration { get; set; }
    
    /// <summary>
    /// Thumbnail blob name if generated
    /// </summary>
    public string? ThumbnailBlobName { get; set; }
    
    /// <summary>
    /// Associated entity type (e.g., "GoodGroup", "Store", "Article")
    /// </summary>
    public string? EntityType { get; set; }
    
    /// <summary>
    /// Associated entity ID
    /// </summary>
    public Guid? EntityId { get; set; }
    
    /// <summary>
    /// Alternative text for accessibility
    /// </summary>
    public string? Alt { get; set; }
    
    /// <summary>
    /// Tags for categorization and search
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Whether file has been processed (thumbnails, conversions, etc.)
    /// </summary>
    public bool IsProcessed { get; set; }
    
    /// <summary>
    /// Whether file is temporary (not confirmed yet)
    /// </summary>
    public bool IsTemp { get; set; }
    
    /// <summary>
    /// File structure key reference (e.g., "Product.MainImage")
    /// </summary>
    public string? StructureKey { get; set; }
    
    /// <summary>
    /// Hash of file content for duplicate detection
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Reference to custom folder (null = in structure path)
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Whether file is archived
    /// </summary>
    public bool IsArchived { get; set; }
    
    /// <summary>
    /// Archive timestamp
    /// </summary>
    public DateTime? ArchivedAt { get; set; }
    
    /// <summary>
    /// Source entity ID (e.g., chat message ID, vision request ID)
    /// </summary>
    public Guid? SourceEntityId { get; set; }
    
    /// <summary>
    /// Custom metadata as serialized JSON
    /// </summary>
    public string? CustomMetadata { get; set; }

    protected FileItem()
    {
    }

    public FileItem(
        Guid id,
        Guid? tenantId,
        string name,
        string originalName,
        string blobName,
        string mimeType,
        long size,
        FileType fileType,
        string? structureKey = null) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        OriginalName = originalName;
        BlobName = blobName;
        MimeType = mimeType;
        Size = size;
        FileType = fileType;
        StructureKey = structureKey;
        IsProcessed = false;
        IsTemp = true;
        IsArchived = false;
    }

    public void SetDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void SetDuration(TimeSpan duration)
    {
        Duration = duration;
    }

    public void SetThumbnail(string thumbnailBlobName)
    {
        ThumbnailBlobName = thumbnailBlobName;
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }

    public void Confirm(Guid? entityId = null)
    {
        IsTemp = false;
        if (entityId.HasValue)
        {
            EntityId = entityId;
        }
    }

    public void AssociateWith(string entityType, Guid entityId)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public void UpdateMetadata(string? alt = null, List<string>? tags = null)
    {
        if (alt != null)
        {
            Alt = alt;
        }
        if (tags != null)
        {
            Tags = tags;
        }
    }
    
    public void Archive(string? reason = null)
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
    }
    
    public void RestoreFromArchive()
    {
        IsArchived = false;
        ArchivedAt = null;
    }
    
    public void SetSourceEntity(Guid? sourceEntityId)
    {
        SourceEntityId = sourceEntityId;
    }

    public void SetCustomMetadata(string? metadata)
    {
        CustomMetadata = metadata;
    }
}
