using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SufiChain.SufiAbp.FileManager.Controllers;

/// <summary>
/// Combined form request for single file upload.
/// IFormFile and metadata combined in one model for Swagger/OpenAPI compatibility.
/// </summary>
/// <remarks>
/// This model is HttpApi-specific because IFormFile is an ASP.NET Core type.
/// The Application layer uses <see cref="FileItems.UploadFileInput"/> with byte[] content instead.
/// </remarks>
public class UploadFileFormRequest
{
    /// <summary>
    /// The file to upload
    /// </summary>
    public IFormFile File { get; set; } = null!;
    
    public string? StructureKey { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    /// <summary>
    /// Target folder ID for file manager (null = root). Used when FolderPath is not set.
    /// </summary>
    public Guid? FolderId { get; set; }
    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// Takes precedence over FolderId. null/empty = root.
    /// </summary>
    public string? FolderPath { get; set; }
    public bool AutoConfirm { get; set; }
    public string? Alt { get; set; }
    
    /// <summary>
    /// Skip processing (thumbnails, conversions) for very large files.
    /// When true, the file is uploaded directly without image/video processing.
    /// </summary>
    public bool SkipProcessing { get; set; }
}

/// <summary>
/// Combined form request for multiple file upload.
/// List of IFormFile and metadata combined in one model for Swagger/OpenAPI compatibility.
/// </summary>
/// <remarks>
/// This model is HttpApi-specific because IFormFile is an ASP.NET Core type.
/// The Application layer uses <see cref="FileItems.UploadMultipleFileInput"/> with byte[] content instead.
/// </remarks>
public class UploadMultipleFilesFormRequest
{
    /// <summary>
    /// The files to upload
    /// </summary>
    public List<IFormFile> Files { get; set; } = new();
    
    public string? StructureKey { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    /// <summary>
    /// Target folder ID for file manager (null = root). Used when FolderPath is not set.
    /// </summary>
    public Guid? FolderId { get; set; }
    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// Takes precedence over FolderId. null/empty = root.
    /// </summary>
    public string? FolderPath { get; set; }
    public bool AutoConfirm { get; set; }
    public string? Alt { get; set; }
}
