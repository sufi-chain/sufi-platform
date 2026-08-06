using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Represents a folder in the file manager hierarchy.
/// Can be a real custom folder or a virtual system folder.
/// </summary>
public class FileFolder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// Tenant ID for multi-tenancy support
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Display name of the folder
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Full path in the folder hierarchy (e.g., "tenant-guid/custom/my-folder/subfolder")
    /// </summary>
    public string Path { get; set; } = default!;

    /// <summary>
    /// Parent folder ID (null for root folders)
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Type of folder (System, Structure, Custom)
    /// </summary>
    public FolderType Type { get; set; }

    /// <summary>
    /// Associated FileStructure key for Structure folders
    /// </summary>
    public string? StructureKey { get; set; }

    /// <summary>
    /// Icon identifier for UI display
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Color code for UI display (hex or named color)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Sort order within parent folder
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether the folder is shared with other tenants
    /// </summary>
    public bool IsShared { get; set; }

    /// <summary>
    /// JSON array of tenant IDs this folder is shared with
    /// </summary>
    public string? SharedWithTenants { get; set; }

    /// <summary>
    /// Optional description of the folder
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property for parent folder
    /// </summary>
    public virtual FileFolder? Parent { get; set; }

    /// <summary>
    /// Navigation property for child folders
    /// </summary>
    public virtual ICollection<FileFolder> Children { get; set; } = new List<FileFolder>();

    /// <summary>
    /// Navigation property for folder permissions
    /// </summary>
    public virtual ICollection<FolderPermission> Permissions { get; set; } = new List<FolderPermission>();

    protected FileFolder()
    {
    }

    public FileFolder(
        Guid id,
        Guid? tenantId,
        string name,
        string path,
        FolderType type,
        Guid? parentId = null,
        string? structureKey = null) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Path = path;
        Type = type;
        ParentId = parentId;
        StructureKey = structureKey;
        SortOrder = 0;
        IsShared = false;
    }

    /// <summary>
    /// Set folder display properties
    /// </summary>
    public void SetDisplayProperties(string? icon, string? color, string? description = null)
    {
        Icon = icon;
        Color = color;
        Description = description;
    }

    /// <summary>
    /// Update folder name and path
    /// </summary>
    public void Rename(string newName, string newPath)
    {
        Name = newName;
        Path = newPath;
    }

    /// <summary>
    /// Move folder to new parent
    /// </summary>
    public void MoveTo(Guid? newParentId, string newPath)
    {
        ParentId = newParentId;
        Path = newPath;
    }

    /// <summary>
    /// Share folder with a tenant
    /// </summary>
    public void ShareWith(List<Guid> tenantIds)
    {
        IsShared = tenantIds.Count > 0;
        SharedWithTenants = tenantIds.Count > 0 
            ? System.Text.Json.JsonSerializer.Serialize(tenantIds) 
            : null;
    }

    /// <summary>
    /// Get list of tenant IDs this folder is shared with
    /// </summary>
    public List<Guid> GetSharedTenantIds()
    {
        if (string.IsNullOrEmpty(SharedWithTenants))
        {
            return new List<Guid>();
        }

        return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(SharedWithTenants) 
            ?? new List<Guid>();
    }

   /// <summary>
   /// Set the sort order for this folder
   /// </summary>
   public void SetSortOrder(int sortOrder)
   {
       SortOrder = sortOrder;
   }

    /// <summary>
    /// Replaces all folder permissions with the given set.
    /// Permissions are mutated through the aggregate root to enforce invariants.
    /// </summary>
    public void SetPermissions(IEnumerable<FolderPermission> permissions)
    {
        Permissions.Clear();
        foreach (var permission in permissions)
        {
            Permissions.Add(permission);
        }
    }

    /// <summary>
    /// Adds a single permission to the folder.
    /// </summary>
    public void AddPermission(FolderPermission permission)
    {
        Permissions.Add(permission);
    }

    /// <summary>
    /// Removes all permissions from the folder.
    /// </summary>
    public void ClearPermissions()
    {
        Permissions.Clear();
    }
}
