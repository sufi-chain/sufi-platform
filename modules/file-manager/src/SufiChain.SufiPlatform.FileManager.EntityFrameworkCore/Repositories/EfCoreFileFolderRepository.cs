using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.FileManager.Repositories;

public class EfCoreFileFolderRepository :
    EfCoreRepository<ISufiFileManagerDbContext, FileFolder, Guid>,
    IFileFolderRepository
{
    public EfCoreFileFolderRepository(IDbContextProvider<ISufiFileManagerDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<FileFolder>> GetRootFoldersAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.ParentId == null && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileFolder>> GetChildrenAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
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
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

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
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .FirstOrDefaultAsync(x => x.Path == path && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<FileFolder>> GetByStructureKeyAsync(
        string structureKey,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.StructureKey == structureKey && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileFolder>> GetDescendantsAsync(
        Guid folderId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var folder = await dbSet.FirstOrDefaultAsync(x => x.Id == folderId, cancellationToken);
        
        if (folder == null)
        {
            return new List<FileFolder>();
        }

        // Get all folders whose path starts with this folder's path
        var pathPrefix = folder.Path + "/";
        return await dbSet
            .Where(x => x.Path.StartsWith(pathPrefix) || x.Id == folderId)
            .OrderBy(x => x.Path)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(
        Guid folderId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.ParentId == folderId, cancellationToken);
    }

    public async Task<FileFolder?> GetWithPermissionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.FileFolders
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<FileFolder>> GetSharedFoldersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var tenantIdString = tenantId.ToString();
        
        return await dbSet
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
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .CountAsync(x => x.ParentId == parentId && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> PathExistsAsync(
        string path,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Path == path && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<FileFolder>> GetByTypeAsync(
        FolderType type,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Type == type && x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}