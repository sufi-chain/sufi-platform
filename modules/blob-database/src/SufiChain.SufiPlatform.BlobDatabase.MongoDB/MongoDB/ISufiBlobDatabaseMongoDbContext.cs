using MongoDB.Driver;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BlobDatabase.MongoDB;

[ConnectionStringName(SufiBlobDatabaseDbProperties.ConnectionStringName)]
public interface ISufiBlobDatabaseMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<DatabaseBlobContainer> BlobContainers { get; }

    IMongoCollection<DatabaseBlob> Blobs { get; }
}
