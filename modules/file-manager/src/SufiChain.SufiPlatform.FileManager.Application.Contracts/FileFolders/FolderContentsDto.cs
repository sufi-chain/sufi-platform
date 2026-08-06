using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Contents of a folder including subfolders and files
/// </summary>
public class FolderContentsDto
{
    /// <summary>
    /// Current folder information
    /// </summary>
    public FolderTreeNodeDto? CurrentFolder { get; set; }

    /// <summary>
    /// Parent folder (null if at root)
    /// </summary>
    public FolderTreeNodeDto? ParentFolder { get; set; }

    /// <summary>
    /// Breadcrumb path from root to current folder
    /// </summary>
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();

    /// <summary>
    /// Subfolders in current folder
    /// </summary>
    public List<FolderTreeNodeDto> Folders { get; set; } = new();

    /// <summary>
    /// Files in current folder
    /// </summary>
    public List<FileItemDto> Files { get; set; } = new();

    /// <summary>
    /// Total count of files (for pagination)
    /// </summary>
    public int TotalFileCount { get; set; }

    /// <summary>
    /// Total count of folders
    /// </summary>
    public int TotalFolderCount { get; set; }

    /// <summary>
    /// Total size of all files in bytes
    /// </summary>
    public long TotalSize { get; set; }
}

/// <summary>
/// Breadcrumb item for folder navigation
/// </summary>
public class BreadcrumbItemDto
{
    /// <summary>
    /// Folder ID (null for virtual folders)
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Full path
    /// </summary>
    public string Path { get; set; } = default!;

    /// <summary>
    /// Icon for display
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Whether this is the current folder
    /// </summary>
    public bool IsCurrent { get; set; }
}
