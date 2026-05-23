using System;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Represents permission assignments for a folder
/// </summary>
public class FolderPermission : Entity<Guid>
{
    /// <summary>
    /// The folder this permission belongs to
    /// </summary>
    public Guid FolderId { get; set; }

    /// <summary>
    /// User ID this permission applies to (if user-specific)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Role ID this permission applies to (if role-based)
    /// </summary>
    public Guid? RoleId { get; set; }

    /// <summary>
    /// The permission level granted
    /// </summary>
    public FolderPermissionLevel Level { get; set; }

    /// <summary>
    /// Whether permissions are inherited by child folders
    /// </summary>
    public bool InheritToChildren { get; set; } = true;

    /// <summary>
    /// Navigation property for the folder
    /// </summary>
    public virtual FileFolder? Folder { get; set; }

    protected FolderPermission()
    {
    }

    public FolderPermission(
        Guid id,
        Guid folderId,
        FolderPermissionLevel level,
        Guid? userId = null,
        Guid? roleId = null) : base(id)
    {
        FolderId = folderId;
        Level = level;
        UserId = userId;
        RoleId = roleId;
    }

    /// <summary>
    /// Create a user-specific permission
    /// </summary>
    public static FolderPermission ForUser(Guid folderId, Guid userId, FolderPermissionLevel level)
    {
        return new FolderPermission(Guid.NewGuid(), folderId, level, userId: userId);
    }

    /// <summary>
    /// Create a role-based permission
    /// </summary>
    public static FolderPermission ForRole(Guid folderId, Guid roleId, FolderPermissionLevel level)
    {
        return new FolderPermission(Guid.NewGuid(), folderId, level, roleId: roleId);
    }

    /// <summary>
    /// Check if the permission includes a specific access level
    /// </summary>
    public bool HasPermission(FolderPermissionLevel requiredLevel)
    {
        return (Level & requiredLevel) == requiredLevel;
    }

    /// <summary>
    /// Update the permission level
    /// </summary>
    public void SetLevel(FolderPermissionLevel level)
    {
        Level = level;
    }
}
