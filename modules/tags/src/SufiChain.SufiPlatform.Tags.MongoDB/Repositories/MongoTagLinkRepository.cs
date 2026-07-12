using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.MongoDB;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.Repositories;

public class MongoTagLinkRepository : MongoDbRepository<ITagsMongoDbContext, TagLink, Guid>, ITagLinkRepository
{
    public MongoTagLinkRepository(IMongoDbContextProvider<ITagsMongoDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public virtual async Task<bool> ExistsAsync(Guid tagId, string entityType, Guid entityId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        var count = await collection.CountDocumentsAsync(
            x => x.TagId == tagId && x.EntityType == entityType && x.EntityId == entityId && x.TenantId == tenantId,
            cancellationToken: cancellationToken);
        return count > 0;
    }

    public virtual async Task<List<TagLink>> GetListByEntityAsync(string entityType, Guid entityId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        return await collection.Find(x => x.EntityType == entityType && x.EntityId == entityId && x.TenantId == tenantId).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TagLink>> GetListByTagAsync(Guid tagId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        return await collection.Find(x => x.TagId == tagId && x.TenantId == tenantId).ToListAsync(cancellationToken);
    }
}
