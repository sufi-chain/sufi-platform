using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.FileManager.Repositories;

public class MongoFileItemRepository :
    MongoDbRepository<IFileManagerMongoDbContext, FileItem, Guid>,
    IFileItemRepository
{
    public MongoFileItemRepository(IMongoDbContextProvider<IFileManagerMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public Task<FileItem> GetAsync(Guid id, CancellationToken cancellationToken = default) => base.GetAsync(id, false, cancellationToken);
    public Task<FileItem?> FindAsync(Guid id, CancellationToken cancellationToken = default) => base.FindAsync(id, false, cancellationToken);
    public Task<IQueryable<FileItem>> GetQueryableAsync(CancellationToken cancellationToken = default) => base.GetQueryableAsync(cancellationToken);

    public Task<FileItem> InsertAsync(FileItem entity, bool autoSave = false, CancellationToken cancellationToken = default) => base.InsertAsync(entity, autoSave, cancellationToken);
    public Task<FileItem> UpdateAsync(FileItem entity, bool autoSave = false, CancellationToken cancellationToken = default) => base.UpdateAsync(entity, autoSave, cancellationToken);

    public async Task<List<FileItem>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileItem>> GetByStructureKeyAsync(
        string structureKey,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.StructureKey == structureKey)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FileItem>> GetTempFilesAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.IsTemp && x.TenantId == tenantId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetTotalSizeByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // Use MongoDB aggregation pipeline for server-side sum calculation
        // This avoids loading all items into memory
        var collection = await GetCollectionAsync(cancellationToken);
        
        var pipeline = new BsonDocument[]
        {
            // Match stage: filter by tenant and non-temp files
            new BsonDocument("$match", new BsonDocument
            {
                { "TenantId", new BsonBinaryData(tenantId, GuidRepresentation.Standard) },
                { "IsTemp", false }
            }),
            // Group stage: sum all Size values
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "TotalSize", new BsonDocument("$sum", "$Size") }
            })
        };

        var result = await collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            return 0;
        }

        return result["TotalSize"].ToInt64();
    }

    public async Task<FileItem?> FindByBlobNameAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
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
        var query = await GetQueryableAsync(cancellationToken);
        query = await ApplySearchFiltersAsync(query, keyword, fileType, entityType, entityId, structureKey, onlyFromPublicStructures, cancellationToken);

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
        var query = await GetQueryableAsync(cancellationToken);
        query = await ApplySearchFiltersAsync(query, keyword, fileType, entityType, entityId, structureKey, onlyFromPublicStructures, cancellationToken);

        return await query.CountAsync(cancellationToken);
    }

    private async Task<IQueryable<FileItem>> ApplySearchFiltersAsync(
        IQueryable<FileItem> query,
        string? keyword,
        FileType? fileType,
        string? entityType,
        Guid? entityId,
        string? structureKey,
        bool? onlyFromPublicStructures,
        CancellationToken cancellationToken)
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
            var dbContext = await GetDbContextAsync(GetCancellationToken(cancellationToken));
            var publicKeys = await dbContext.FileStructures
                .AsQueryable()
                .Where(s => s.IsPublicAccess)
                .Select(s => s.Key)
                .ToListAsync(cancellationToken);
            query = query.Where(x => x.StructureKey != null && publicKeys.Contains(x.StructureKey));
        }

        return query;
    }
}
