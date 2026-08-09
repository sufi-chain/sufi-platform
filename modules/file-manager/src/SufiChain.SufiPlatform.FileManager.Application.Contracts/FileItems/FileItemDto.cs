using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public class FileItemDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string OriginalName { get; set; } = default!;
    public string BlobName { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public long Size { get; set; }
    public FileStructureStorageProvider? StorageProvider { get; set; }
    public FileType FileType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ThumbnailBlobName { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Alt { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool StructureIsPublicAccess { get; set; }
    public string? StructureBaseUrl { get; set; }
    /// <summary>Storage provider for the structure (e.g. S3Provider). Used to build direct public URLs when IsPublicAccess and BaseUrl.</summary>
    public string? StructureStorageProvider { get; set; }
    public bool IsProcessed { get; set; }
    public bool IsTemp { get; set; }
    public string? StructureKey { get; set; }

    /// <summary>
    /// Parent folder ID (null = root / structure path).
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Indicates if the file is archived.
    /// </summary>
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public Guid? SourceEntityId { get; set; }
    public string? SourceEntityType { get; set; }
    public Dictionary<string, string>? CustomMetadata { get; set; }
}
