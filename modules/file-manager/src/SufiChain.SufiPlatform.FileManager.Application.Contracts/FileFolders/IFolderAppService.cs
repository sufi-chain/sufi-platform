using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Application service for folder management operations
/// </summary>
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
public interface IFolderAppService : IApplicationService
{
    #region Tree Operations

    /// <summary>
    /// Get folder tree for navigation
    /// </summary>
    Task<List<FolderTreeNodeDto>> GetTreeAsync(Guid? tenantId = null);

    /// <summary>
    /// Get children of a folder (lazy loading)
    /// </summary>
    Task<List<FolderTreeNodeDto>> GetChildrenAsync(Guid? parentId, string? parentPath = null);

    /// <summary>
    /// Get folder contents (files and subfolders)
    /// </summary>
    Task<FolderContentsDto> GetContentsAsync(GetFolderContentsInput input);

    /// <summary>
    /// Get folder by ID
    /// </summary>
    Task<FileFolderDto> GetAsync(Guid id);

    /// <summary>
    /// Get folder by path
    /// </summary>
    Task<FileFolderDto?> GetByPathAsync(string path);

    /// <summary>
    /// Ensures the folder path exists, creating any missing parent folders.
    /// E.g. "/web/tourist" creates "web" under root if needed, then "tourist" under "web".
    /// Returns the folder at the final path, or null if path is empty/root.
    /// </summary>
    Task<FileFolderDto?> GetOrCreateFolderByPathAsync(string path);

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Create a new folder
    /// </summary>
    Task<FileFolderDto> CreateAsync(CreateFolderInput input);

    /// <summary>
    /// Rename a folder
    /// </summary>
    Task<FileFolderDto> RenameAsync(Guid id, RenameFolderInput input);

    /// <summary>
    /// Delete a folder
    /// </summary>
    Task DeleteAsync(Guid id, bool recursive = false);

    /// <summary>
    /// Move a folder to a new parent
    /// </summary>
    Task<FileFolderDto> MoveAsync(Guid id, MoveFolderInput input);

    /// <summary>
    /// Copy a folder and its contents
    /// </summary>
    Task<FileFolderDto> CopyAsync(Guid id, Guid? targetParentId);

    #endregion

    #region Permissions

    /// <summary>
    /// Set permissions for a folder
    /// </summary>
    Task SetPermissionsAsync(Guid folderId, SetFolderPermissionsInput input);

    /// <summary>
    /// Get permissions for a folder
    /// </summary>
    Task<List<FolderPermissionDto>> GetPermissionsAsync(Guid folderId);

    /// <summary>
    /// Check if current user has permission
    /// </summary>
    Task<bool> HasPermissionAsync(Guid folderId, FolderPermissionLevelDto level);

    #endregion

    #region Sharing

    /// <summary>
    /// Share folder with tenants
    /// </summary>
    Task ShareAsync(Guid folderId, ShareFolderInput input);

    /// <summary>
    /// Unshare folder with a tenant
    /// </summary>
    Task UnshareAsync(Guid folderId, Guid tenantId);

    /// <summary>
    /// Get folders shared with current tenant
    /// </summary>
    Task<List<FolderTreeNodeDto>> GetSharedFoldersAsync();

    #endregion

    #region Statistics

    /// <summary>
    /// Get folder statistics
    /// </summary>
    Task<FolderStatisticsDto> GetStatisticsAsync(Guid? folderId, string? path = null);

    #endregion
}
