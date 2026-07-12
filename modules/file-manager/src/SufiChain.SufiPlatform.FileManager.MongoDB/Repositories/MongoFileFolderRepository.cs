using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.FileManager.Repositories;

public class MongoFileFolderRepository :
    MongoDbRepository<IFileManagerMongoDbContext, FileFolder, Guid>,
    IFileFolderRepository
{
    public MongoFileFolderRepository(IMongoDbContextProvider<IFileManagerMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<FileFolder>> GetRootFoldersAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.ParentId == null && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileFolder>> GetChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileFolder>> GetFolderTreeAsync(
        Guid? tenantId = null,
        bool includeShared = true,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);

        if (includeShared && tenantId.HasValue)
        {
            // Include owned folders and shared folders
            var tenantIdString = tenantId.Value.ToString();
            query = query.Where(x =>
                x.TenantId == tenantId ||
                (x.IsShared && x.SharedWithTenants != null && x.SharedWithTenants.Contains(tenantIdString)));
        }
        else
        {
            query = query.Where(x => x.TenantId == tenantId);
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FileFolder?> FindByPathAsync(
        string path,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.Path == path && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<FileFolder>> GetByStructureKeyAsync(
        string structureKey,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.StructureKey == structureKey && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileFolder>> GetDescendantsAsync(
        Guid folderId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        var folder = await query.FirstOrDefaultAsync(x => x.Id == folderId, cancellationToken);

        if (folder == null)
        {
            return new List<FileFolder>();
        }

        // Get all folders whose path starts with this folder's path
        var pathPrefix = folder.Path + "/";
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.Path.StartsWith(pathPrefix) || x.Id == folderId)
            .OrderBy(x => x.Path)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(
        Guid folderId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .AnyAsync(x => x.ParentId == folderId, cancellationToken);
    }

    public async Task<FileFolder?> GetWithPermissionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't have Include like EF Core
        // Permissions would need to be fetched separately or embedded
        // For now, just return the folder - permissions are stored in the same document
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<FileFolder>> GetSharedFoldersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenantIdString = tenantId.ToString();

        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.IsShared && x.SharedWithTenants != null && x.SharedWithTenants.Contains(tenantIdString))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetChildCountAsync(
        Guid? parentId,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .CountAsync(x => x.ParentId == parentId && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> PathExistsAsync(
        string path,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .AnyAsync(x => x.Path == path && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<FileFolder>> GetByTypeAsync(
        FolderType type,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.Type == type && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
