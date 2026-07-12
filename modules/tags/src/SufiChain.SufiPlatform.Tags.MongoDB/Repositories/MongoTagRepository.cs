using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.MongoDB;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.Repositories;

public class MongoTagRepository : MongoDbRepository<ITagsMongoDbContext, Tags.Tag, Guid>, ITagRepository
{
    public MongoTagRepository(IMongoDbContextProvider<ITagsMongoDbContext> dbContextProvider) : base(dbContextProvider)
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

    public virtual async Task<List<Tags.Tag>> SearchAsync(
        string? scope,
        string? filter,
        Guid? tenantId = null,
        int skipCount = 0,
        int maxResultCount = 20,
        CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        var builder = Builders<Tags.Tag>.Filter;
        var filters = new List<FilterDefinition<Tags.Tag>>
        {
            builder.Eq(x => x.TenantId, tenantId)
        };

        if (!string.IsNullOrWhiteSpace(scope))
        {
            filters.Add(builder.Eq(x => x.Scope, scope));
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var trimmedFilter = filter.Trim();
            var normalizedFilter = trimmedFilter.ToUpperInvariant();
            filters.Add(builder.Or(
                builder.Regex(x => x.Name, new BsonRegularExpression(trimmedFilter, "i")),
                builder.Eq(x => x.NormalizedName, normalizedFilter)));
        }

        return await collection.Find(builder.And(filters))
            .SortBy(x => x.Name)
            .Skip(skipCount)
            .Limit(maxResultCount)
            .ToListAsync(cancellationToken);
    }
}
