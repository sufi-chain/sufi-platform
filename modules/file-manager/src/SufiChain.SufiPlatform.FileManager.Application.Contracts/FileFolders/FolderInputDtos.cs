using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Input for creating a new folder
/// </summary>
public class CreateFolderInput
{
    /// <summary>
    /// Display name of the folder
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Parent folder ID (null for root folder)
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Optional parent path for virtual folders
    /// </summary>
    public string? ParentPath { get; set; }

    /// <summary>
    /// Icon identifier
    /// </summary>
    [StringLength(64)]
    public string? Icon { get; set; }

    /// <summary>
    /// Color code
    /// </summary>
    [StringLength(32)]
    public string? Color { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    [StringLength(1024)]
    public string? Description { get; set; }
}

/// <summary>
/// Input for renaming a folder
/// </summary>
public class RenameFolderInput
{
    /// <summary>
    /// New name for the folder
    /// </summary>
    [Required]
    [StringLength(256)]
    public string NewName { get; set; } = default!;
}

/// <summary>
/// Input for moving a folder
/// </summary>
public class MoveFolderInput
{
    /// <summary>
    /// Target parent folder ID (null to move to root)
    /// </summary>
    public Guid? NewParentId { get; set; }

    /// <summary>
    /// Target parent path for virtual folders
    /// </summary>
    public string? NewParentPath { get; set; }
}

/// <summary>
/// Input for getting folder contents
/// </summary>
public class GetFolderContentsInput
{
    /// <summary>
    /// Folder ID to get contents of
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Virtual path to get contents of
    /// </summary>
    public string? VirtualPath { get; set; }

    /// <summary>
    /// Number of files to skip (pagination)
    /// </summary>
    public int SkipCount { get; set; }

    /// <summary>
    /// Maximum number of files to return
    /// </summary>
    public int MaxResultCount { get; set; } = 50;

    /// <summary>
    /// Sort by field
    /// </summary>
    public string? Sorting { get; set; }

    /// <summary>
    /// Filter by file name
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Filter files by structure key (null = show all)
    /// </summary>
    public string? StructureKey { get; set; }

    /// <summary>
    /// Explorer source mode. DirectoryMap uses FileFolder mappings; BlobPath groups files by BlobName path.
    /// </summary>
    public FileExplorerSourceMode SourceMode { get; set; } = FileExplorerSourceMode.DirectoryMap;
}

/// <summary>
/// File explorer content source.
/// </summary>
public enum FileExplorerSourceMode
{
    DirectoryMap = 0,
    BlobPath = 1
}

/// <summary>
/// Input for setting folder permissions
/// </summary>
public class SetFolderPermissionsInput
{
    /// <summary>
    /// List of permissions to set
    /// </summary>
    public List<FolderPermissionDto> Permissions { get; set; } = new();
}

/// <summary>
/// DTO for folder permission
/// </summary>
public class FolderPermissionDto
{
   public Guid? Id { get; set; }
   public Guid? UserId { get; set; }
   public Guid? RoleId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
   public FolderPermissionLevelDto Level { get; set; }
   public bool InheritToChildren { get; set; } = true;
}

/// <summary>
/// Permission level enum for DTOs
/// </summary>
[Flags]
public enum FolderPermissionLevelDto
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Share = 8,
    Full = Read | Write | Delete | Share
}

/// <summary>
/// Input for sharing folder with tenants
/// </summary>
public class ShareFolderInput
{
    /// <summary>
    /// List of tenant IDs to share with
    /// </summary>
    public List<Guid> TenantIds { get; set; } = new();
}
