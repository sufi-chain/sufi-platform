using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BlobStoring.Database.MongoDB;

public class MongoDbDatabaseBlobRepository : MongoDbRepository<ISufiAbpBlobStoringMongoDbContext, DatabaseBlob, Guid>, IDatabaseBlobRepository
{
    public MongoDbDatabaseBlobRepository(IMongoDbContextProvider<ISufiAbpBlobStoringMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<DatabaseBlob?> FindAsync(Guid containerId, string name, CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);

        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.ContainerId == containerId && x.Name == name, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Guid containerId, string name, CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);

        return await (await GetQueryableAsync(cancellationToken))
            .AnyAsync(x => x.ContainerId == containerId && x.Name == name, cancellationToken);
    }

    public virtual async Task<bool> DeleteAsync(Guid containerId, string name, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var blob = await FindAsync(containerId, name, cancellationToken);
        if (blob == null)
        {
            return false;
        }

        await base.DeleteAsync(blob, autoSave, GetCancellationToken(cancellationToken));
        return true;
    }
}
