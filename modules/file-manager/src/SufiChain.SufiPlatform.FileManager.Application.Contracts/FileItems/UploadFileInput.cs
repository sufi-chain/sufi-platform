using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public class UploadFileInput
{
    [Required]
    public string FileName { get; set; } = default!;

    [Required]
    public byte[] Content { get; set; } = default!;

    [Required]
    public string MimeType { get; set; } = default!;

    public string? StructureKey { get; set; }
    
    public string? EntityType { get; set; }
    
    public Guid? EntityId { get; set; }
    
    /// <summary>
    /// Target folder ID for file manager uploads (used when FolderPath is not set)
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// Takes precedence over FolderId. null/empty = root.
    /// </summary>
    public string? FolderPath { get; set; }
    
    public bool AutoConfirm { get; set; } = false;
    
    public string? Alt { get; set; }
}

/// <summary>
/// Input for streaming file uploads (memory efficient for large files)
/// </summary>
public class UploadFileStreamInput
{
    [Required]
    public string FileName { get; set; } = default!;

    /// <summary>
    /// Stream containing the file content. Will be disposed by the caller.
    /// </summary>
    [Required]
    public Stream ContentStream { get; set; } = default!;

    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long ContentLength { get; set; }

    [Required]
    public string MimeType { get; set; } = default!;

    public string? StructureKey { get; set; }
    
    public string? EntityType { get; set; }
    
    public Guid? EntityId { get; set; }
    
    /// <summary>
    /// Target folder ID for file manager uploads (used when FolderPath is not set)
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// Takes precedence over FolderId. null/empty = root.
    /// </summary>
    public string? FolderPath { get; set; }
    
    public bool AutoConfirm { get; set; } = false;
    
    public string? Alt { get; set; }
    
    /// <summary>
    /// Skip processing (thumbnails, conversions) for very large files
    /// </summary>
    public bool SkipProcessing { get; set; }
}

public class UploadMultipleFileInput
{
    [Required]
    public string? StructureKey { get; set; }
    
    public string? EntityType { get; set; }
    
    public Guid? EntityId { get; set; }
    
    /// <summary>
    /// Target folder ID for file manager uploads (null = root). Used when FolderPath is not set.
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// Takes precedence over FolderId. null/empty = root.
    /// </summary>
    public string? FolderPath { get; set; }
    
    public bool AutoConfirm { get; set; } = false;
    
    [Required]
    public FileInput[] Files { get; set; } = Array.Empty<FileInput>();
}

public class FileInput
{
    [Required]
    public string FileName { get; set; } = default!;

    [Required]
    public byte[] Content { get; set; } = default!;

    [Required]
    public string MimeType { get; set; } = default!;
    
    public string? Alt { get; set; }
}

/// <summary>
/// Input for deleting multiple file items
/// </summary>
public class DeleteManyFileItemsInput
{
    [Required]
    public Guid[] Ids { get; set; } = Array.Empty<Guid>();
}

/// <summary>
/// Result of upload validation (mime type, extension, size) without throwing.
/// Used by the upload controller to return 400 with message instead of letting the app service throw.
/// </summary>
public class UploadValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
