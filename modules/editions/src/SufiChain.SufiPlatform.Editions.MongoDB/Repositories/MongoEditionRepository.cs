using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Editions.MongoDB.Repositories;

public class MongoEditionRepository : MongoDbRepository<IEditionsMongoDbContext, Edition, Guid>, IEditionRepository
{
    public MongoEditionRepository(IMongoDbContextProvider<IEditionsMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<Edition?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await (await GetMongoQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public virtual async Task<Edition?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await (await GetMongoQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);
    }
}
