using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.FileManager.Repositories;

public class EfCoreFileItemRepository :
    EfCoreRepository<ISufiFileManagerDbContext, FileItem, Guid>,
    IFileItemRepository
{
    public EfCoreFileItemRepository(IDbContextProvider<ISufiFileManagerDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public Task<FileItem> GetAsync(Guid id, CancellationToken cancellationToken = default) => base.GetAsync(id, false, cancellationToken);
    public Task<FileItem?> FindAsync(Guid id, CancellationToken cancellationToken = default) => base.FindAsync(id, false, cancellationToken);
    public Task<IQueryable<FileItem>> GetQueryableAsync(CancellationToken cancellationToken = default) => base.GetQueryableAsync();

    public async Task<List<FileItem>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileItem>> GetByStructureKeyAsync(
        string structureKey,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.StructureKey == structureKey)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileItem>> GetTempFilesAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.IsTemp && x.TenantId == tenantId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetTotalSizeByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.TenantId == tenantId)
            .SumAsync(x => (long?)x.Size, cancellationToken) ?? 0;
    }

    public async Task<FileItem?> FindByBlobNameAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .FirstOrDefaultAsync(x => x.BlobName == blobName, cancellationToken);
    }

    public async Task<List<FileItem>> SearchAsync(
        string? keyword = null,
        FileType? fileType = null,
        string? entityType = null,
        Guid? entityId = null,
        string? structureKey = null,
        bool? onlyFromPublicStructures = null,
        int skipCount = 0,
        int maxResultCount = 10,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var dbContext = await GetDbContextAsync();
        var query = dbSet.AsQueryable();

        query = ApplySearchFilters(dbContext, query, keyword, fileType, entityType, entityId, structureKey, onlyFromPublicStructures);

        return await query
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? keyword = null,
        FileType? fileType = null,
        string? entityType = null,
        Guid? entityId = null,
        string? structureKey = null,
        bool? onlyFromPublicStructures = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var dbContext = await GetDbContextAsync();
        var query = dbSet.AsQueryable();

        query = ApplySearchFilters(dbContext, query, keyword, fileType, entityType, entityId, structureKey, onlyFromPublicStructures);

        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<FileItem> ApplySearchFilters(
        ISufiFileManagerDbContext dbContext,
        IQueryable<FileItem> query,
        string? keyword,
        FileType? fileType,
        string? entityType,
        Guid? entityId,
        string? structureKey,
        bool? onlyFromPublicStructures)
    {
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.OriginalName.Contains(keyword) ||
                x.Name.Contains(keyword) ||
                (x.Alt != null && x.Alt.Contains(keyword)));
        }

        if (fileType.HasValue)
        {
            query = query.Where(x => x.FileType == fileType.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(structureKey))
        {
            query = query.Where(x => x.StructureKey == structureKey);
        }

        if (onlyFromPublicStructures == true)
        {
            var publicKeyQuery = dbContext.FileStructures.Where(s => s.IsPublicAccess).Select(s => s.Key);
            query = query.Where(x => x.StructureKey != null && publicKeyQuery.Contains(x.StructureKey));
        }

        return query;
    }
}
