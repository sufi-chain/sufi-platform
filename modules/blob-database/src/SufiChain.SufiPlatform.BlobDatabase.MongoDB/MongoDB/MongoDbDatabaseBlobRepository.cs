using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BlobDatabase.MongoDB;

public class MongoDbDatabaseBlobRepository : MongoDbRepository<ISufiBlobDatabaseMongoDbContext, DatabaseBlob, Guid>, IDatabaseBlobRepository
{
    public MongoDbDatabaseBlobRepository(IMongoDbContextProvider<ISufiBlobDatabaseMongoDbContext> dbContextProvider)
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
