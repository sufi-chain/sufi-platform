using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Timing;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.Repos;

public class MongoShortUrlRepository : MongoDbRepository<IShortLinkGeneratorMongoDbContext, ShortUrl, Guid>, IShortUrlRepository
{
    private readonly IClock _clock;

    public MongoShortUrlRepository(
        IMongoDbContextProvider<IShortLinkGeneratorMongoDbContext> dbContextProvider,
        IClock clock)
        : base(dbContextProvider)
    {
        _clock = clock;
    }
    
    public virtual async Task<ShortUrl?> FindByShortCodeAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken);
    }
    
    public virtual async Task<bool> ShortCodeExistsAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .AnyAsync(x => x.ShortCode == shortCode, cancellationToken);
    }
    
    public virtual async Task<List<ShortUrl>> GetExpiredUrlsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = _clock.Now;
        return await (await GetQueryableAsync())
            .Where(x => x.ExpiresAt.HasValue && x.ExpiresAt < now && x.IsActive)
            .ToListAsync(cancellationToken);
    }
    
    public virtual async Task IncrementClickCountAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync(cancellationToken);
        var filter = Builders<ShortUrl>.Filter.Eq(x => x.Id, id);
        var update = Builders<ShortUrl>.Update
            .Inc(x => x.ClickCount, 1)
            .Set(x => x.LastAccessedAt, _clock.Now);
            
        await collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}

