using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BlobStoring.Database.MongoDB;

public class MongoDbDatabaseBlobContainerRepository : MongoDbRepository<ISufiAbpBlobStoringMongoDbContext, DatabaseBlobContainer, Guid>, IDatabaseBlobContainerRepository
{
    public MongoDbDatabaseBlobContainerRepository(IMongoDbContextProvider<ISufiAbpBlobStoringMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<DatabaseBlobContainer?> FindAsync(string name, CancellationToken cancellationToken = default)
    {
        return await base.FindAsync(x => x.Name == name, cancellationToken: GetCancellationToken(cancellationToken));
    }
}
