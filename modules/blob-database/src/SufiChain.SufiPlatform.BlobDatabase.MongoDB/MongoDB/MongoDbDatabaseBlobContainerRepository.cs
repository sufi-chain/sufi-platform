using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BlobDatabase.MongoDB;

public class MongoDbDatabaseBlobContainerRepository : MongoDbRepository<ISufiBlobDatabaseMongoDbContext, DatabaseBlobContainer, Guid>, IDatabaseBlobContainerRepository
{
    public MongoDbDatabaseBlobContainerRepository(IMongoDbContextProvider<ISufiBlobDatabaseMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<DatabaseBlobContainer?> FindAsync(string name, CancellationToken cancellationToken = default)
    {
        return await base.FindAsync(x => x.Name == name, cancellationToken: GetCancellationToken(cancellationToken));
    }
}
