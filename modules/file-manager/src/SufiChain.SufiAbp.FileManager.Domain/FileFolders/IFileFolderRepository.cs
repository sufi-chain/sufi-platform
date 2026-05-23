using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Repository interface for FileFolder entity
/// </summary>
public interface IFileFolderRepository : IRepository<FileFolder, Guid>, ITransientDependency
{
    /// <summary>
    /// Get all root folders for a tenant (folders with no parent)
    /// </summary>
    Task<List<FileFolder>> GetRootFoldersAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get children of a folder
    /// </summary>
    Task<List<FileFolder>> GetChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get full folder tree for a tenant
    /// </summary>
    Task<List<FileFolder>> GetFolderTreeAsync(
        Guid? tenantId = null,
        bool includeShared = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get folder by path
    /// </summary>
    Task<FileFolder?> FindByPathAsync(
        string path,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get folders by structure key
    /// </summary>
    Task<List<FileFolder>> GetByStructureKeyAsync(
        string structureKey,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get folder with all descendants (for recursive operations)
    /// </summary>
    Task<List<FileFolder>> GetDescendantsAsync(
        Guid folderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a folder has children
    /// </summary>
    Task<bool> HasChildrenAsync(
        Guid folderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get folder with permissions
    /// </summary>
    Task<FileFolder?> GetWithPermissionsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get folders shared with a specific tenant
    /// </summary>
    Task<List<FileFolder>> GetSharedFoldersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count folders in a parent
    /// </summary>
    Task<int> GetChildCountAsync(
        Guid? parentId,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if path exists for a tenant
    /// </summary>
    Task<bool> PathExistsAsync(
        string path,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get folders by type
    /// </summary>
    Task<List<FileFolder>> GetByTypeAsync(
        FolderType type,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);
}
