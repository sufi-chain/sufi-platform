using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using SufiChain.SufiAbp.TagsManagement.MongoDB;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TagsManagement.Repositories;

public class MongoTagRepository : MongoDbRepository<ITagsManagementMongoDbContext, Tags.Tag, Guid>, ITagRepository
{
    public MongoTagRepository(IMongoDbContextProvider<ITagsManagementMongoDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public virtual async Task<Tags.Tag?> FindByNameAsync(string scope, string normalizedName, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        return await collection.Find(x => x.Scope == scope && x.NormalizedName == normalizedName && x.TenantId == tenantId).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<Tags.Tag>> GetListByScopeAsync(string scope, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        return await collection.Find(x => x.Scope == scope && x.TenantId == tenantId).ToListAsync(cancellationToken);
    }
}
