using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Tree node representation of a folder for hierarchical display
/// </summary>
public class FolderTreeNodeDto
{
    /// <summary>
    /// Folder ID (null for virtual folders like tenant root)
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
    /// Parent folder ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Type of folder
    /// </summary>
    public FolderTypeDto Type { get; set; }

    /// <summary>
    /// Icon for display
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Color for display
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Whether the folder has children
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// Number of direct child folders
    /// </summary>
    public int ChildFolderCount { get; set; }

    /// <summary>
    /// Number of files in this folder
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Total size of files in bytes
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Whether this is a virtual folder (not stored in DB)
    /// </summary>
    public bool IsVirtual { get; set; }

    /// <summary>
    /// Associated structure key for structure folders
    /// </summary>
    public string? StructureKey { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenant scenarios
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Tenant name for display (host only)
    /// </summary>
    public string? TenantName { get; set; }

    /// <summary>
    /// Whether the folder is expanded in UI
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Child nodes (lazy loaded)
    /// </summary>
    public List<FolderTreeNodeDto>? Children { get; set; }

    /// <summary>
    /// Whether the folder is shared
    /// </summary>
    public bool IsShared { get; set; }

    /// <summary>
    /// Whether the current user can write to this folder
    /// </summary>
    public bool CanWrite { get; set; }

    /// <summary>
    /// Whether the current user can delete this folder
    /// </summary>
    public bool CanDelete { get; set; }
}
