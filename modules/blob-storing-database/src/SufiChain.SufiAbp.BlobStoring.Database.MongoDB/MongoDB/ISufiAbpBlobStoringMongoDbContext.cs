using MongoDB.Driver;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BlobStoring.Database.MongoDB;

[ConnectionStringName(SufiAbpBlobStoringDatabaseDbProperties.ConnectionStringName)]
public interface ISufiAbpBlobStoringMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<DatabaseBlobContainer> BlobContainers { get; }

    IMongoCollection<DatabaseBlob> Blobs { get; }
}
